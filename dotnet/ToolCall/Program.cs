// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using Microsoft.AI.Foundry.Local;
using OpenAI;
using OpenAI.Chat;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Parse command line arguments
            string scenario = "sms";
            string modelAlias = "deepseek-r1-distill-qwen-7b-cuda-gpu";
            string userMessage = "";
            int maxTokens = 2048;
            bool verbose = false;
            bool help = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--scenario":
                    case "-s":
                        if (i + 1 < args.Length)
                            scenario = args[++i].ToLower();
                        break;
                    case "--model":
                    case "-m":
                        if (i + 1 < args.Length)
                            modelAlias = args[++i];
                        break;
                    case "--message":
                    case "-msg":
                        if (i + 1 < args.Length)
                            userMessage = args[++i];
                        break;
                    case "--max-tokens":
                    case "-t":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out var tokens))
                            maxTokens = tokens;
                        break;
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                    case "--help":
                    case "-h":
                    case "/?":
                        help = true;
                        break;
                }
            }

            if (help)
            {
                ShowHelp();
                return 0;
            }

            // Special commands for model information
            if (scenario == "model-info" || scenario == "resolve")
            {
                await ShowModelResolution(modelAlias, verbose);
                return 0;
            }
            else if (scenario == "catalog")
            {
                await ShowCatalog(verbose);
                return 0;
            }

            if (verbose)
            {
                Console.WriteLine($"Scenario: {scenario}");
                Console.WriteLine($"Using model: {modelAlias}");
                if (!string.IsNullOrEmpty(userMessage))
                    Console.WriteLine($"User message: {userMessage}");
                Console.WriteLine($"Max tokens: {maxTokens}");
                Console.WriteLine();
            }

            // Initialize the model and client
            var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: modelAlias);
            var model = await manager.GetModelInfoAsync(aliasOrModelId: modelAlias);
            ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
            OpenAIClient client = new OpenAIClient(
                key,
                new OpenAIClientOptions { Endpoint = manager.Endpoint }
            );

            var chatClient = client.GetChatClient(model?.ModelId);

            // Execute the appropriate scenario
            switch (scenario)
            {
                case "sms":
                    await RunSmsScenario(chatClient, userMessage, maxTokens, verbose);
                    break;
                case "weather":
                    await RunWeatherScenario(chatClient, userMessage, maxTokens, verbose);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown scenario: {scenario}");
                    Console.WriteLine("Available scenarios: sms, weather");
                    return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }

    static async Task RunSmsScenario(
        ChatClient chatClient,
        string userMessage,
        int maxTokens,
        bool verbose
    )
    {
        if (verbose)
            Console.WriteLine("Running SMS scenario...");

        // Use default message if not provided
        if (string.IsNullOrEmpty(userMessage))
            userMessage = "'I'd like to order 10 'Clean Code' books' to 666-111-222";

        // Create messages
        var messages = new ChatMessage[]
        {
            ChatMessage.CreateSystemMessage(
                @"You are a help desk assistant with tools. When you need to call a tool, output ONLY a JSON object with the tool name and parameters.
                Example: {""tool"": ""SendSms"", ""message"": ""your message"", ""phoneNumber"": ""123-456-7890""}
                Do not include any other text, markdown formatting, or code fences - just the raw JSON object."
            ),
            ChatMessage.CreateUserMessage(userMessage),
        };

        // Create tools and handlers
        var tools = new List<ChatTool> { SmsService.GetTool() };
        var toolHandlers = new Dictionary<string, Func<Dictionary<string, object?>, string>>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["SendSms"] = args =>
            {
                if (
                    args.TryGetValue("message", out var msg)
                    && args.TryGetValue("phoneNumber", out var phone)
                )
                {
                    return SmsService.SendSms(msg?.ToString() ?? "", phone?.ToString() ?? "");
                }
                return "Error: Missing required parameters for SendSms";
            },
        };

        // Execute the chat completion with tools
        var result = await ExecuteChatWithTools(
            chatClient,
            messages,
            tools,
            toolHandlers,
            maxTokens
        );

        // Display results
        DisplayChatResult(result);
    }

    static async Task RunWeatherScenario(
        ChatClient chatClient,
        string userMessage,
        int maxTokens,
        bool verbose
    )
    {
        if (verbose)
            Console.WriteLine("Running Weather scenario...");

        // Use default message if not provided
        if (string.IsNullOrEmpty(userMessage))
            userMessage = "What's the weather like today?";

        List<ChatMessage> messages = [new UserChatMessage(userMessage)];

        ChatCompletionOptions options = new()
        {
            MaxOutputTokenCount = maxTokens,
            Tools =
            {
                WeatherService.GetCurrentLocationTool(),
                WeatherService.GetCurrentWeatherTool(),
            },
        };

        bool requiresAction;

        do
        {
            requiresAction = false;
            ChatCompletion completion = chatClient.CompleteChat(messages, options);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                {
                    // Add the assistant message to the conversation history.
                    messages.Add(new AssistantChatMessage(completion));
                    break;
                }

                case ChatFinishReason.ToolCalls:
                {
                    // First, add the assistant message with tool calls to the conversation history.
                    messages.Add(new AssistantChatMessage(completion));

                    // Then, add a new tool message for each tool call that is resolved.
                    foreach (ChatToolCall toolCall in completion.ToolCalls)
                    {
                        switch (toolCall.FunctionName)
                        {
                            case nameof(WeatherService.GetCurrentLocation):
                            {
                                string toolResult = WeatherService.GetCurrentLocation();
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            }

                            case nameof(WeatherService.GetCurrentWeather):
                            {
                                // The arguments that the model wants to use to call the function are specified as a
                                // stringified JSON object based on the schema defined in the tool definition. Note that
                                // the model may hallucinate arguments too. Consequently, it is important to do the
                                // appropriate parsing and validation before calling the function.
                                using JsonDocument argumentsJson = JsonDocument.Parse(
                                    toolCall.FunctionArguments
                                );
                                bool hasLocation = argumentsJson.RootElement.TryGetProperty(
                                    "location",
                                    out JsonElement location
                                );
                                bool hasUnit = argumentsJson.RootElement.TryGetProperty(
                                    "unit",
                                    out JsonElement unit
                                );

                                if (!hasLocation)
                                {
                                    throw new ArgumentNullException(
                                        nameof(location),
                                        "The location argument is required."
                                    );
                                }

                                string toolResult = hasUnit
                                    ? WeatherService.GetCurrentWeather(
                                        location.GetString(),
                                        unit.GetString()
                                    )
                                    : WeatherService.GetCurrentWeather(location.GetString());
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            }

                            default:
                            {
                                // Handle other unexpected calls.
                                throw new NotImplementedException();
                            }
                        }
                    }

                    requiresAction = true;
                    break;
                }

                case ChatFinishReason.Length:
                    throw new NotImplementedException(
                        "Incomplete model output due to MaxTokens parameter or token limit exceeded."
                    );

                case ChatFinishReason.ContentFilter:
                    throw new NotImplementedException(
                        "Omitted content due to a content filter flag."
                    );

                case ChatFinishReason.FunctionCall:
                    throw new NotImplementedException("Deprecated in favor of tool calls.");

                default:
                    throw new NotImplementedException(completion.FinishReason.ToString());
            }
        } while (requiresAction);

        // Display the final conversation
        Console.WriteLine("=== Weather Scenario Result ===");
        foreach (var msg in messages)
        {
            if (msg is UserChatMessage userMsg)
                Console.WriteLine($"User: {userMsg.Content[0].Text}");
            else if (msg is AssistantChatMessage assistantMsg)
                Console.WriteLine(
                    $"Assistant: {assistantMsg.Content?.FirstOrDefault()?.Text ?? ""}"
                );
        }
    }

    static async Task ShowCatalog(bool verbose)
    {
        Console.WriteLine("=== Model Catalog Query (No Download) ===\n");
        
        try
        {
            var manager = new FoundryLocalManager();
            
            // Need to start the service first
            Console.WriteLine("Starting Foundry Local service...");
            await manager.StartServiceAsync();
            
            Console.WriteLine("Querying catalog (this does NOT download models)...\n");
            
            // Call the catalog listing method
            var catalogModels = await manager.ListCatalogModelsAsync();
            
            if (catalogModels != null && catalogModels.Any())
            {
                Console.WriteLine($"📚 Found {catalogModels.Count()} models in catalog:\n");
                
                // Group by alias
                var grouped = catalogModels.GroupBy(m => m.Alias ?? "no-alias");
                
                foreach (var group in grouped)
                {
                    Console.WriteLine($"Alias: '{group.Key}'");
                    foreach (var model in group)
                    {
                        var deviceIcon = model.Runtime?.ExecutionProvider == ExecutionProvider.CUDAExecutionProvider ? "🎮" : "💻";
                        Console.WriteLine($"  {deviceIcon} {model.ModelId}");
                        Console.WriteLine($"     Device: {model.Runtime?.DeviceType}");
                        Console.WriteLine($"     Provider: {model.Runtime?.ExecutionProvider}");
                        Console.WriteLine($"     Size: {model.FileSizeMb} MB");
                        
                        if (verbose)
                        {
                            Console.WriteLine($"     Type: {model.ModelType}");
                            Console.WriteLine($"     Publisher: {model.Publisher}");
                        }
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No models found in catalog");
            }
            
            // Also show cached and loaded models
            Console.WriteLine("\n📦 Cached Models (already downloaded):");
            var cachedModels = await manager.ListCachedModelsAsync();
            if (cachedModels?.Any() == true)
            {
                foreach (var model in cachedModels)
                {
                    Console.WriteLine($"  - {model.ModelId}");
                }
            }
            else
            {
                Console.WriteLine("  None");
            }
            
            Console.WriteLine("\n🔥 Loaded Models (currently in memory):");
            var loadedModels = await manager.ListLoadedModelsAsync();
            if (loadedModels?.Any() == true)
            {
                foreach (var model in loadedModels)
                {
                    Console.WriteLine($"  - {model.ModelId}");
                }
            }
            else
            {
                Console.WriteLine("  None");
            }
            
            await manager.DisposeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }
        }
    }

    static async Task ShowModelResolution(string modelAlias, bool verbose)
    {
        Console.WriteLine($"=== Model Resolution for alias: '{modelAlias}' ===\n");
        Console.WriteLine($"🔍 Resolving model alias on your system...\n");

        try
        {
            // Start the model to see what gets selected
            Console.WriteLine($"Starting model with alias: {modelAlias}");
            var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: modelAlias);
            
            Console.WriteLine($"Model started successfully!");
            Console.WriteLine($"Retrieving model information...\n");
            
            var modelInfo = await manager.GetModelInfoAsync(aliasOrModelId: modelAlias);
            
            if (modelInfo != null)
            {
                Console.WriteLine($"✅ Model Resolution Complete:");
                Console.WriteLine($"   Alias requested: {modelAlias}");
                Console.WriteLine($"   Actual Model ID: {modelInfo.ModelId ?? "N/A"}");
                
                // Explain the selection based on the model ID
                if (modelInfo.ModelId?.Contains("cuda-gpu") == true)
                {
                    Console.WriteLine($"\n🎯 Hardware Match:");
                    Console.WriteLine($"   GPU-optimized variant selected: {modelInfo.ModelId}");
                    Console.WriteLine($"   This variant uses CUDA for GPU acceleration");
                    Console.WriteLine($"   Perfect for systems with NVIDIA GPUs like RTX 4090!");
                }
                else if (modelInfo.ModelId?.Contains("cuda") == true)
                {
                    Console.WriteLine($"\n🎯 Hardware Match:");
                    Console.WriteLine($"   CUDA variant selected: {modelInfo.ModelId}");
                    Console.WriteLine($"   This will use NVIDIA GPU acceleration");
                }
                else if (modelInfo.ModelId?.Contains("cpu") == true)
                {
                    Console.WriteLine($"\n💻 CPU Variant:");
                    Console.WriteLine($"   CPU-optimized variant selected: {modelInfo.ModelId}");
                    Console.WriteLine($"   This variant will run on CPU (no GPU acceleration)");
                }
                else
                {
                    Console.WriteLine($"\n📦 Model Variant:");
                    Console.WriteLine($"   Selected: {modelInfo.ModelId}");
                }
                
                // Try to show any additional info using reflection or ToString
                if (verbose)
                {
                    Console.WriteLine($"\n📊 Model Info Object:");
                    Console.WriteLine($"   Type: {modelInfo.GetType().FullName}");
                    Console.WriteLine($"   ToString: {modelInfo.ToString()}");
                    
                    // Use reflection to see what properties are available
                    var properties = modelInfo.GetType().GetProperties();
                    if (properties.Length > 0)
                    {
                        Console.WriteLine($"\n   Available Properties:");
                        foreach (var prop in properties)
                        {
                            try
                            {
                                var value = prop.GetValue(modelInfo);
                                if (value != null)
                                {
                                    Console.WriteLine($"   - {prop.Name}: {value}");
                                }
                            }
                            catch
                            {
                                // Skip properties that can't be read
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("⚠️  Model started but no detailed information available");
                Console.WriteLine($"   The model is running at: {manager.Endpoint}");
            }
            
            // Show connection info
            Console.WriteLine($"\n🔌 Connection Details:");
            Console.WriteLine($"   Endpoint: {manager.Endpoint}");
            Console.WriteLine($"   API Key: {manager.ApiKey?.Substring(0, Math.Min(8, manager.ApiKey?.Length ?? 0))}...");
            
            await manager.DisposeAsync();
            Console.WriteLine($"\n✅ Model manager disposed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error resolving model: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("ToolCall - AI Chat with Tool Calling");
        Console.WriteLine();
        Console.WriteLine("Usage: ToolCall [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine(
            "  -s, --scenario <name>    Scenario to run: sms, weather, model-info (default: sms)"
        );
        Console.WriteLine(
            "  -m, --model <alias>      Model alias or ID to use (default: deepseek-r1-distill-qwen-7b-cuda-gpu)"
        );
        Console.WriteLine("  -msg, --message <text>   User message to send to the AI");
        Console.WriteLine("  -t, --max-tokens <n>     Maximum output tokens (default: 2048)");
        Console.WriteLine("  -v, --verbose            Enable verbose output");
        Console.WriteLine("  -h, --help               Show this help message");
        Console.WriteLine();
        Console.WriteLine("Scenarios:");
        Console.WriteLine("  sms        - SMS sending scenario (default message: order books)");
        Console.WriteLine("  weather    - Weather query scenario (default message: Seattle weather)");
        Console.WriteLine("  model-info - Show which model variant gets selected (WARNING: downloads model)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine(
            "  ToolCall --scenario sms --message \"Send an SMS to 555-1234 about the meeting\""
        );
        Console.WriteLine("  ToolCall -s weather -msg \"What's the temperature in New York?\"");
        Console.WriteLine("  ToolCall -s model-info -m deepseek-r1-7b");
        Console.WriteLine("  ToolCall -m gpt-4 -s sms -msg \"Order 5 books\" --verbose");
    }

    // Standalone function to execute chat completion with tools
    static async Task<ChatResult> ExecuteChatWithTools(
        ChatClient chatClient,
        IList<ChatMessage> messages,
        IList<ChatTool> tools,
        Dictionary<string, Func<Dictionary<string, object?>, string>> toolHandlers,
        int maxOutputTokenCount = 2048
    )
    {
        var result = new ChatResult();

        try
        {
            // Configure options with tools
            ChatCompletionOptions options = new() { MaxOutputTokenCount = maxOutputTokenCount };

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
                    content = Regex
                        .Replace(content, thinkPattern, "", RegexOptions.Singleline)
                        .Trim();
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
                        IsProperToolCall = true,
                    };

                    // Parse arguments and execute
                    try
                    {
                        var argsDict = ParseJsonToDict(toolCall.FunctionArguments.ToString());
                        result.ToolCallResult = ExecuteToolCall(
                            toolCall.FunctionName,
                            argsDict,
                            toolHandlers
                        );
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
    static ChatResult ParseToolCallFromContent(
        string content,
        ChatResult result,
        Dictionary<string, Func<Dictionary<string, object?>, string>> toolHandlers
    )
    {
        var jsonPattern = @"```json\s*(.*?)\s*```|(\{.*?\})";
        var match = Regex.Match(content, jsonPattern, RegexOptions.Singleline);

        if (match.Success)
        {
            var jsonContent = match.Groups[1].Success
                ? match.Groups[1].Value
                : match.Groups[2].Value;

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
                            _ => prop.Value.ToString(),
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
                        IsProperToolCall = false,
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
                _ => prop.Value.ToString(),
            };
        }

        return argsDict;
    }

    // Execute tool call using provided handlers
    static string ExecuteToolCall(
        string functionName,
        Dictionary<string, object?> args,
        Dictionary<string, Func<Dictionary<string, object?>, string>> toolHandlers
    )
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
            Console.WriteLine(
                $"🔧 Tool Call ({(result.ToolCall.IsProperToolCall ? "Structured" : "Parsed from content")}):"
            );
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
// Service classes
public class WeatherService
{
    private static readonly ChatTool getCurrentLocationTool = ChatTool.CreateFunctionTool(
        functionName: nameof(GetCurrentLocation),
        functionDescription: "Get the user's current location"
    );

    private static readonly ChatTool getCurrentWeatherTool = ChatTool.CreateFunctionTool(
        functionName: nameof(GetCurrentWeather),
        functionDescription: "Get the current weather in a given location",
        functionParameters: BinaryData.FromBytes(
            """
            {
                "type": "object",
                "properties": {
                    "location": {
                        "type": "string",
                        "description": "The city and state, e.g. Boston, MA"
                    },
                    "unit": {
                        "type": "string",
                        "enum": [ "celsius", "fahrenheit" ],
                        "description": "The temperature unit to use. Infer this from the specified location."
                    }
                },
                "required": [ "location" ]
            }
            """u8.ToArray()
        )
    );

    public static ChatTool GetCurrentLocationTool() => getCurrentLocationTool;

    public static ChatTool GetCurrentWeatherTool() => getCurrentWeatherTool;

    public static string GetCurrentLocation()
    {
        // Call the location API here.
        return "San Francisco";
    }

    public static string GetCurrentWeather(string location, string unit = "celsius")
    {
        // Call the weather API here.
        return $"31 {unit}";
    }
}

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
