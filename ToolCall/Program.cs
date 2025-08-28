// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AI.Foundry.Local;
using OpenAI;
using OpenAI.Chat;

// Initialize the model and client
var alias = "deepseek-r1-distill-qwen-7b-cuda-gpu";

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);
var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);
ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(
    key,
    new OpenAIClientOptions { Endpoint = manager.Endpoint }
);

var chatClient = client.GetChatClient(model?.ModelId);

// Create test messages
var messages = new ChatMessage[]
{
    ChatMessage.CreateSystemMessage(
        @"You are a help desk assistant with tools. When you need to call a tool, output ONLY a JSON object with the tool name and parameters.
        Example: {""tool"": ""SendSms"", ""message"": ""your message"", ""phoneNumber"": ""123-456-7890""}
        Do not include any other text, markdown formatting, or code fences - just the raw JSON object."
    ),
    ChatMessage.CreateUserMessage("'I'd like to order 10 'Clean Code' books' to 666-111-222"),
};

// Create tools and handlers
var tools = new List<ChatTool> { SmsService.GetTool() };
var toolHandlers = new Dictionary<string, Func<Dictionary<string, object?>, string>>(StringComparer.OrdinalIgnoreCase)
{
    ["SendSms"] = args => 
    {
        if (args.TryGetValue("message", out var msg) && args.TryGetValue("phoneNumber", out var phone))
        {
            return SmsService.SendSms(msg?.ToString() ?? "", phone?.ToString() ?? "");
        }
        return "Error: Missing required parameters for SendSms";
    }
};

// Execute the chat completion with tools
var result = await ExecuteChatWithTools(chatClient, messages, tools, toolHandlers);

// Display results
DisplayChatResult(result);

// Standalone function to execute chat completion with tools
static async Task<ChatResult> ExecuteChatWithTools(
    ChatClient chatClient,
    IList<ChatMessage> messages,
    IList<ChatTool> tools,
    Dictionary<string, Func<Dictionary<string, object?>, string>> toolHandlers,
    int maxOutputTokenCount = 2048)
{
    var result = new ChatResult();
    
    try
    {
        // Configure options with tools
        ChatCompletionOptions options = new() 
        { 
            MaxOutputTokenCount = maxOutputTokenCount 
        };
        
        foreach (var tool in tools)
        {
            options.Tools.Add(tool);
        }

        // Execute chat completion
        var completion = await Task.Run(() => chatClient.CompleteChat(messages, options));

        if (completion.Value != null)
        {
            var response = completion.Value;
            
            // Get the assistant message content
            var content = response.Content?.FirstOrDefault()?.Text ?? "";
            
            // Extract thinking content
            var thinkPattern = @"<think>(.*?)</think>";
            var thinkMatch = Regex.Match(content, thinkPattern, RegexOptions.Singleline);
            if (thinkMatch.Success)
            {
                result.Thoughts = thinkMatch.Groups[1].Value.Trim();
                // Remove thinking from main content
                content = Regex.Replace(content, thinkPattern, "", RegexOptions.Singleline).Trim();
            }
            
            result.Content = content;
            
            // Check for proper tool calls
            if (response.ToolCalls.Count > 0)
            {
                var toolCall = response.ToolCalls.First();
                result.ToolCall = new ToolCallInfo
                {
                    Id = toolCall.Id,
                    FunctionName = toolCall.FunctionName,
                    Arguments = toolCall.FunctionArguments.ToString(),
                    IsProperToolCall = true
                };
                
                // Parse arguments and execute
                try
                {
                    var argsDict = ParseJsonToDict(toolCall.FunctionArguments.ToString());
                    result.ToolCallResult = ExecuteToolCall(toolCall.FunctionName, argsDict, toolHandlers);
                }
                catch (Exception ex)
                {
                    result.ToolCallResult = $"Error executing tool: {ex.Message}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(content))
            {
                // Try to parse tool call from content
                result = ParseToolCallFromContent(content, result, toolHandlers);
            }
        }
    }
    catch (Exception ex)
    {
        result.Content = $"Error during chat completion: {ex.Message}";
    }
    
    return result;
}

// Parse tool call from content
static ChatResult ParseToolCallFromContent(string content, ChatResult result, Dictionary<string, Func<Dictionary<string, object?>, string>> toolHandlers)
{
    var jsonPattern = @"```json\s*(.*?)\s*```|(\{.*?\})";
    var match = Regex.Match(content, jsonPattern, RegexOptions.Singleline);
    
    if (match.Success)
    {
        var jsonContent = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
        
        try
        {
            var json = JsonDocument.Parse(jsonContent);
            
            // Parse the JSON to extract tool name and arguments
            string functionName = "";
            Dictionary<string, object?> argsDict = new Dictionary<string, object?>();
            
            // Check if it has a "tool" or "function" field
            if (json.RootElement.TryGetProperty("tool", out var toolName))
            {
                functionName = toolName.GetString() ?? "";
            }
            else if (json.RootElement.TryGetProperty("function", out var funcName))
            {
                functionName = funcName.GetString() ?? "";
            }
            else if (json.RootElement.TryGetProperty("name", out var name))
            {
                functionName = name.GetString() ?? "";
            }
            
            // Extract all other properties as arguments
            foreach (var prop in json.RootElement.EnumerateObject())
            {
                // Skip the tool/function/name field
                if (prop.Name != "tool" && prop.Name != "function" && prop.Name != "name")
                {
                    argsDict[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        JsonValueKind.Array => prop.Value.ToString(),
                        JsonValueKind.Object => prop.Value.ToString(),
                        _ => prop.Value.ToString()
                    };
                }
            }
            
            // If we found a tool/function name and have arguments
            if (!string.IsNullOrEmpty(functionName) && argsDict.Count > 0)
            {
                var cleanArgs = JsonSerializer.Serialize(argsDict);
                
                result.ToolCall = new ToolCallInfo
                {
                    FunctionName = functionName,
                    Arguments = cleanArgs,
                    IsProperToolCall = false
                };
                
                // Execute the tool call dynamically
                result.ToolCallResult = ExecuteToolCall(functionName, argsDict, toolHandlers);
                
                // Clear content if it only contained the tool call
                if (match.Value == content.Trim())
                {
                    result.Content = null;
                }
            }
        }
        catch (Exception ex)
        {
            // Not valid JSON or not a tool call
            Console.WriteLine($"Debug: Failed to parse JSON from content: {ex.Message}");
        }
    }
    
    return result;
}

// Parse JSON string to dictionary
static Dictionary<string, object?> ParseJsonToDict(string jsonString)
{
    var argsDict = new Dictionary<string, object?>();
    var argsDoc = JsonDocument.Parse(jsonString);
    
    foreach (var prop in argsDoc.RootElement.EnumerateObject())
    {
        argsDict[prop.Name] = prop.Value.ValueKind switch
        {
            JsonValueKind.String => prop.Value.GetString(),
            JsonValueKind.Number => prop.Value.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => prop.Value.ToString(),
            JsonValueKind.Object => prop.Value.ToString(),
            _ => prop.Value.ToString()
        };
    }
    
    return argsDict;
}

// Execute tool call using provided handlers
static string ExecuteToolCall(
    string functionName, 
    Dictionary<string, object?> args, 
    Dictionary<string, Func<Dictionary<string, object?>, string>> toolHandlers)
{
    try
    {
        if (toolHandlers.TryGetValue(functionName, out var handler))
        {
            return handler(args);
        }
        return $"Error: Unknown tool '{functionName}'";
    }
    catch (Exception ex)
    {
        return $"Error executing {functionName}: {ex.Message}";
    }
}

// Display chat result
static void DisplayChatResult(ChatResult result)
{
    Console.WriteLine("=== Chat Completion Result ===\n");

    if (!string.IsNullOrEmpty(result.Thoughts))
    {
        Console.WriteLine("💭 Thinking:");
        Console.WriteLine($"   {result.Thoughts.Replace("\n", "\n   ")}\n");
    }

    if (!string.IsNullOrEmpty(result.Content))
    {
        Console.WriteLine("📝 Content:");
        Console.WriteLine($"   {result.Content}\n");
    }

    if (result.ToolCall != null)
    {
        Console.WriteLine($"🔧 Tool Call ({(result.ToolCall.IsProperToolCall ? "Structured" : "Parsed from content")}):");
        Console.WriteLine($"   Function: {result.ToolCall.FunctionName}");
        Console.WriteLine($"   Arguments: {result.ToolCall.Arguments}");
        if (!string.IsNullOrEmpty(result.ToolCall.Id))
        {
            Console.WriteLine($"   ID: {result.ToolCall.Id}");
        }
        Console.WriteLine();
    }

    if (!string.IsNullOrEmpty(result.ToolCallResult))
    {
        Console.WriteLine("✅ Tool Call Result:");
        Console.WriteLine($"   {result.ToolCallResult}\n");
    }
}

// Result classes
public class ChatResult
{
    public string? Thoughts { get; set; }
    public string? Content { get; set; }
    public ToolCallInfo? ToolCall { get; set; }
    public string? ToolCallResult { get; set; }
}

public class ToolCallInfo
{
    public string? Id { get; set; }
    public string FunctionName { get; set; } = "";
    public string Arguments { get; set; } = "";
    public bool IsProperToolCall { get; set; }
}

// Service classes
public class SmsService
{
    public static ChatTool GetTool()
    {
        ChatTool tool = ChatTool.CreateFunctionTool(
            nameof(SendSms),
            "send SMS",
            BinaryData.FromString(
                @"
            {
              ""type"": ""object"",
              ""properties"": {
                ""message"": {
                  ""type"": ""string"",
                  ""description"": ""text of SMS""
                },
                ""phoneNumber"": {
                  ""type"": ""string"",
                  ""description"": ""phone number of recipient""
                }
              },
              ""required"": [""message"", ""phoneNumber""]
            }
        "
            )
        );

        return tool;
    }

    public static string SendSms(string message, string phoneNumber)
    {
        return $"SMS sent to {phoneNumber}: '{message}'";
    }
}