// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AI.Foundry.Local;
using OpenAI;
using OpenAI.Chat;

// var alias = "deepseek-r1-distill-qwen-7b-generic-gpu:3";
var alias = "Phi-4-mini-instruct-generic-gpu";

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);
var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);

Console.WriteLine("=== Testing via REST API ===");
Console.WriteLine($"Endpoint: {manager.Endpoint}");
Console.WriteLine($"API Key: {manager.ApiKey}");
Console.WriteLine($"Model ID: {model?.ModelId}");

// Create HTTP client for REST API
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {manager.ApiKey}");

// Build the request body
var requestBody = new
{
    model = model?.ModelId,
    messages = new[]
    {
        new
        {
            role = "system",
            content = "You are a helpful assistant. You have access to tools to help users. Use the available tools when appropriate to fulfill user requests.",
        },
        new
        {
            role = "user",
            content = "Send an SMS to 555-123-4567 saying 'Meeting moved to 3pm'",
        },
    },
    tools = new[]
    {
        new
        {
            type = "function",
            function = new
            {
                name = "SendSms",
                description = "send SMS",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        message = new { type = "string", description = "text of SMS" },
                        phoneNumber = new
                        {
                            type = "string",
                            description = "phone number of recipient",
                        },
                    },
                    required = new[] { "message", "phoneNumber" },
                },
            },
        },
    },
    max_tokens = 2048,
};

var json = JsonSerializer.Serialize(
    requestBody,
    new JsonSerializerOptions { WriteIndented = true }
);
Console.WriteLine("\n=== Request Body ===");
Console.WriteLine(json);

var content = new StringContent(json, Encoding.UTF8, "application/json");
var url = $"{manager.Endpoint}/chat/completions";
Console.WriteLine($"\n=== Calling: {url} ===");

try
{
    var response = await httpClient.PostAsync(url, content);
    var responseBody = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"\n=== Response Status: {response.StatusCode} ===");
    Console.WriteLine("\n=== Raw Response ===");
    Console.WriteLine(responseBody);

    // Try to parse and pretty-print the response
    try
    {
        var responseJson = JsonDocument.Parse(responseBody);
        var prettyJson = JsonSerializer.Serialize(
            responseJson,
            new JsonSerializerOptions { WriteIndented = true }
        );
        Console.WriteLine("\n=== Pretty Response ===");
        Console.WriteLine(prettyJson);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not parse response as JSON: {ex.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error calling API: {ex.Message}");
}

Console.WriteLine("\n\n=== Now testing via OpenAI SDK (original code) ===");

ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(
    key,
    new OpenAIClientOptions { Endpoint = manager.Endpoint }
);

var chatClient = client.GetChatClient(model?.ModelId);

var messages = new ChatMessage[]
{
    ChatMessage.CreateSystemMessage(
        "You are a helpful assistant. You have access to tools to help users. Use the available tools when appropriate to fulfill user requests."
    ),
    ChatMessage.CreateUserMessage("Send an SMS to 555-123-4567 saying 'Meeting moved to 3pm'"),
};

var tool = SmsService.GetTool();

ChatCompletionOptions options = new() { Tools = { tool }, MaxOutputTokenCount = 2048 };

Console.WriteLine($"[DEBUG] Tools configured: {options.Tools.Count}");
Console.WriteLine($"[DEBUG] Tool name: {tool.FunctionName}");

var completionUpdates = chatClient.CompleteChatStreaming(messages, options);

Console.Write($"[ASSISTANT]: ");
foreach (var completionUpdate in completionUpdates)
{
    // Check for text content
    if (completionUpdate.ContentUpdate.Count > 0)
    {
        Console.Write(completionUpdate.ContentUpdate[0].Text);
    }

    // Check for tool calls - THIS IS CRITICAL!
    if (completionUpdate.ToolCallUpdates.Count > 0)
    {
        foreach (var toolCall in completionUpdate.ToolCallUpdates)
        {
            Console.WriteLine($"\n[TOOL CALL DETECTED]: {toolCall.FunctionName}");
            if (toolCall.FunctionArgumentsUpdate != null)
            {
                Console.WriteLine($"Arguments: {toolCall.FunctionArgumentsUpdate.ToString()}");
            }
        }
    }
}
Console.WriteLine();

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
        return "SMS sent!";
    }
}
