using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text.Json;
using Microsoft.AI.Foundry.Local;
using OpenAI;
using OpenAI.Chat;

public class Program
{
    // Define the PowerShell execution tool
    static readonly ChatTool runPowerShellScriptTool = ChatTool.CreateFunctionTool(
        functionName: nameof(RunPowerShellScript),
        functionDescription: "Execute a PowerShell script and return its output",
        functionParameters: BinaryData.FromBytes("""
        {
            "type": "object",
            "properties": {
                "script": {
                    "type": "string",
                    "description": "The PowerShell script code to execute"
                },
                "description": {
                    "type": "string",
                    "description": "Brief description of what the script does"
                }
            },
            "required": [ "script" ]
        }
        """u8.ToArray())
    );

    // Function to execute PowerShell scripts
    static string RunPowerShellScript(string script, string? description = null)
    {
        if (!string.IsNullOrEmpty(description))
        {
            Console.WriteLine($"\n[Tool] Executing: {description}");
        }
        else
        {
            Console.WriteLine("\n[Tool] Executing PowerShell script...");
        }
        
        using PowerShell ps = PowerShell.Create();
        ps.AddScript(script);
        
        try
        {
            Collection<PSObject> results = ps.Invoke();
            
            // Check for errors
            if (ps.HadErrors)
            {
                var errors = new List<string>();
                foreach (var error in ps.Streams.Error)
                {
                    errors.Add($"Error: {error}");
                }
                return $"Script execution failed:\n{string.Join("\n", errors)}";
            }
            
            // Format results
            var output = new List<string>();
            foreach (PSObject result in results)
            {
                // Handle different result types
                if (result.BaseObject is string str)
                {
                    output.Add(str);
                }
                else
                {
                    // For complex objects, format as properties
                    var props = new List<string>();
                    foreach (var prop in result.Properties)
                    {
                        if (prop.Value != null)
                        {
                            props.Add($"{prop.Name}: {prop.Value}");
                        }
                    }
                    if (props.Any())
                    {
                        output.Add(string.Join(", ", props));
                    }
                }
            }
            
            string resultOutput = output.Any() ? string.Join("\n", output) : "Script executed successfully with no output";
            Console.WriteLine($"[Tool Result]\n{resultOutput}\n");
            return resultOutput;
        }
        catch (Exception ex)
        {
            var error = $"PowerShell execution error: {ex.Message}";
            Console.WriteLine($"[Tool Error] {error}");
            return error;
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("PSAgent - PowerShell Agent with AI");
        Console.WriteLine("\nUsage:");
        Console.WriteLine("  PSAgent [model-alias]                    - Start interactive REPL mode");
        Console.WriteLine("  PSAgent [model-alias] \"prompt\"            - Execute single prompt");
        Console.WriteLine("  PSAgent --help                            - Show this help");
        Console.WriteLine("\nExamples:");
        Console.WriteLine("  PSAgent                                   - Start REPL with default model");
        Console.WriteLine("  PSAgent \"list running services\"           - Single command with default model");
        Console.WriteLine("  PSAgent phi-3.5-mini \"get system info\"    - Single command with specific model");
        Console.WriteLine("\nAvailable models can be listed when the agent starts.");
    }

    public static async Task Main(string[] args)
    {
        // Parse command line arguments
        if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h" || args[0] == "/?"))
        {
            PrintHelp();
            return;
        }

        string? modelAlias = null;
        string? singlePrompt = null;
        
        // Determine mode based on arguments
        if (args.Length == 0)
        {
            // No args: REPL with default model
            modelAlias = "qwen2.5-coder-7b-instruct-generic-gpu";
        }
        else if (args.Length == 1)
        {
            // One arg: could be model or prompt
            if (args[0].Contains(" ") || !args[0].Contains("-"))
            {
                // Likely a prompt (contains spaces or doesn't look like a model name)
                singlePrompt = args[0];
                modelAlias = "qwen2.5-coder-7b-instruct-generic-gpu";
            }
            else
            {
                // Likely a model name
                modelAlias = args[0];
            }
        }
        else if (args.Length == 2)
        {
            // Two args: model and prompt
            modelAlias = args[0];
            singlePrompt = args[1];
        }
        else
        {
            // More than 2 args: join remaining as prompt
            modelAlias = args[0];
            singlePrompt = string.Join(" ", args.Skip(1));
        }

        try
        {
            Console.WriteLine($"Initializing Foundry Local with model: {modelAlias}");

            var manager = await FoundryLocalManager.StartModelAsync(modelAlias);

            // Verify service is running
            if (!manager.IsServiceRunning)
            {
                throw new InvalidOperationException("Failed to start Foundry Local service");
            }

            // Get model info with proper null checking
            var modelInfo = await manager.GetModelInfoAsync(modelAlias);
            if (modelInfo == null)
            {
                // List available models if requested model not found
                var catalog = await manager.ListCatalogModelsAsync();
                throw new InvalidOperationException(
                    $"Model '{modelAlias}' not found. Available models: "
                        + string.Join(", ", catalog.Select(m => m.Alias))
                );
            }

            Console.WriteLine($"Model loaded: {modelInfo.ModelId}");
            Console.WriteLine($"Service endpoint: {manager.Endpoint}");

            // Configure OpenAI client to use local endpoint
            var client = new OpenAIClient(
                new System.ClientModel.ApiKeyCredential(manager.ApiKey),
                new OpenAIClientOptions { Endpoint = manager.Endpoint }
            );

            var chatClient = client.GetChatClient(modelInfo.ModelId);

            // Create tool-enabled completion handler
            var toolCompletionHandler = new ToolCallingCompletionHandler(chatClient);
            
            // Register the PowerShell tool
            toolCompletionHandler.RegisterTool(
                "RunPowerShellScript",
                (Dictionary<string, object> args) =>
                {
                    var script = args.ContainsKey("script") ? args["script"].ToString() : "";
                    var description = args.ContainsKey("description") ? args["description"].ToString() : null;
                    return RunPowerShellScript(script!, description);
                }
            );

            var systemPrompt = "You are a helpful PowerShell assistant. When asked to perform system tasks or retrieve system information, " +
                              "use the RunPowerShellScript tool to execute PowerShell commands. " +
                              "Always provide a clear description of what the script does. " +
                              "Be concise in your responses.";

            if (!string.IsNullOrEmpty(singlePrompt))
            {
                // Single prompt mode
                Console.WriteLine($"\nPrompt: {singlePrompt}\n");
                var response = await toolCompletionHandler.CompleteChatWithToolsAsync(systemPrompt, singlePrompt);
                Console.WriteLine($"Response: {response}");
            }
            else
            {
                // REPL mode
                Console.WriteLine("\n=== PSAgent Interactive Mode ===");
                Console.WriteLine("Type 'exit', 'quit', or press Ctrl+C to quit");
                Console.WriteLine("Type 'clear' to clear the screen");
                Console.WriteLine("Type 'help' for usage information\n");

                var conversationHistory = new List<ChatMessage>();
                conversationHistory.Add(new SystemChatMessage(systemPrompt));

                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(input))
                        continue;
                    
                    // Handle special commands
                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    
                    if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Clear();
                        Console.WriteLine("=== PSAgent Interactive Mode ===\n");
                        continue;
                    }
                    
                    if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("\nAvailable commands:");
                        Console.WriteLine("  exit/quit - Exit the program");
                        Console.WriteLine("  clear     - Clear the screen");
                        Console.WriteLine("  help      - Show this help");
                        Console.WriteLine("\nYou can ask me to:");
                        Console.WriteLine("  - Get system information");
                        Console.WriteLine("  - List processes, services, files");
                        Console.WriteLine("  - Execute PowerShell commands");
                        Console.WriteLine("  - Perform system administration tasks\n");
                        continue;
                    }
                    
                    // Process user input with conversation context
                    var response = await toolCompletionHandler.CompleteChatWithContextAsync(
                        conversationHistory, 
                        input
                    );
                    
                    Console.WriteLine($"\n{response}\n");
                }
            }

            // Cleanup
            Console.WriteLine("\nShutting down Foundry Local service...");
            await manager.StopServiceAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }
}

// Abstraction layer for tool-calling completions
// When real tool calling is supported, only this class needs to be modified
public class ToolCallingCompletionHandler
{
    private readonly ChatClient _chatClient;
    private readonly Dictionary<string, Func<Dictionary<string, object>, string>> _tools;
    private readonly bool _useNativeToolCalling;

    public ToolCallingCompletionHandler(ChatClient chatClient, bool useNativeToolCalling = false)
    {
        _chatClient = chatClient;
        _tools = new Dictionary<string, Func<Dictionary<string, object>, string>>();
        _useNativeToolCalling = useNativeToolCalling; // Set to true when native support is available
    }

    public void RegisterTool(string toolName, Func<Dictionary<string, object>, string> toolFunction)
    {
        _tools[toolName] = toolFunction;
    }

    public async Task<string> CompleteChatWithToolsAsync(string systemPrompt, string userPrompt, float temperature = 0.1f)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };
        
        return await ProcessWithToolsAsync(messages, temperature);
    }

    public async Task<string> CompleteChatWithContextAsync(
        List<ChatMessage> conversationHistory, 
        string userPrompt, 
        float temperature = 0.1f)
    {
        // Add user message to history
        conversationHistory.Add(new UserChatMessage(userPrompt));
        
        // Process with tools
        var response = await ProcessWithToolsAsync(conversationHistory, temperature);
        
        // Add assistant response to history
        conversationHistory.Add(new AssistantChatMessage(response));
        
        return response;
    }

    private async Task<string> ProcessWithToolsAsync(List<ChatMessage> messages, float temperature)
    {
        if (_useNativeToolCalling)
        {
            // Future implementation: Use native OpenAI tool calling
            return await ProcessWithNativeToolsAsync(messages, temperature);
        }
        else
        {
            // Current implementation: Simulate tool calling with JSON
            return await ProcessWithJsonToolsAsync(messages, temperature);
        }
    }

    private async Task<string> ProcessWithNativeToolsAsync(List<ChatMessage> messages, float temperature)
    {
        // This will be implemented when Foundry Local supports native tool calling
        // For now, just redirect to JSON-based version
        return await ProcessWithJsonToolsAsync(messages, temperature);
    }

    private async Task<string> ProcessWithJsonToolsAsync(List<ChatMessage> messages, float temperature)
    {
        // Build tool descriptions for the prompt
        var toolDescriptions = "\n\nYou have access to the following tools:\n\n";
        foreach (var tool in _tools.Keys)
        {
            toolDescriptions += $"- {tool}: Execute operations using this tool\n";
        }
        
        toolDescriptions += "\nTo use a tool, respond with ONLY a JSON object in this format:\n" +
                           "{\n" +
                           "  \"tool\": \"ToolName\",\n" +
                           "  \"parameters\": {\n" +
                           "    \"param1\": \"value1\",\n" +
                           "    \"param2\": \"value2\"\n" +
                           "  }\n" +
                           "}\n\n" +
                           "For the RunPowerShellScript tool specifically, use these parameters:\n" +
                           "- script (string, required): The PowerShell script to execute\n" +
                           "- description (string, optional): Description of what the script does\n\n" +
                           "Output ONLY the JSON when you want to use a tool, otherwise respond normally.";

        // Add tool descriptions to system message if not already present
        var workingMessages = new List<ChatMessage>(messages);
        if (workingMessages[0] is SystemChatMessage sysMsg && !sysMsg.Content[0].Text.Contains("You have access to the following tools"))
        {
            workingMessages[0] = new SystemChatMessage(sysMsg.Content[0].Text + toolDescriptions);
        }

        var options = new ChatCompletionOptions
        {
            Temperature = temperature,
            TopP = 0.9f
        };

        // Get initial response
        var completion = await _chatClient.CompleteChatAsync(workingMessages, options);
        
        if (completion.Value.Content == null || completion.Value.Content.Count == 0)
        {
            return "No response from model.";
        }

        var response = completion.Value.Content[0].Text ?? "";
        
        // Try to detect and execute tool calls
        var toolResult = await TryExecuteToolFromResponse(response);
        
        if (toolResult != null)
        {
            // Tool was executed, get final response
            workingMessages.Add(new AssistantChatMessage(response));
            workingMessages.Add(new UserChatMessage($"Tool execution result:\n{toolResult}\n\nPlease provide a summary of the results."));
            
            var finalCompletion = await _chatClient.CompleteChatAsync(workingMessages, options);
            if (finalCompletion.Value.Content != null && finalCompletion.Value.Content.Count > 0)
            {
                return finalCompletion.Value.Content[0].Text ?? "";
            }
        }

        return response;
    }

    private async Task<string?> TryExecuteToolFromResponse(string response)
    {
        // Check if response looks like it contains JSON
        if (!response.Contains("{") || !response.Contains("\"tool\"") && !response.Contains("\"function\""))
        {
            return null;
        }

        try
        {
            // Clean up response
            response = response.Replace("```json", "").Replace("```", "").Trim();
            
            // Find JSON in response
            int jsonStart = response.IndexOf('{');
            int jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return null;
            }

            var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            using var doc = JsonDocument.Parse(jsonStr);
            
            string? toolName = null;
            Dictionary<string, object>? parameters = null;

            // Parse tool call format
            if (doc.RootElement.TryGetProperty("tool", out var toolElement))
            {
                toolName = toolElement.GetString();
                
                if (doc.RootElement.TryGetProperty("parameters", out var paramsElement))
                {
                    parameters = ParseParameters(paramsElement);
                }
            }
            else if (doc.RootElement.TryGetProperty("function", out var funcElement))
            {
                // Alternative format support
                if (funcElement.TryGetProperty("name", out var nameElement))
                {
                    toolName = nameElement.GetString();
                }
                
                if (funcElement.TryGetProperty("parameters", out var paramsElement))
                {
                    parameters = ParseParameters(paramsElement);
                }
            }

            // Execute tool if found
            if (!string.IsNullOrEmpty(toolName) && _tools.ContainsKey(toolName))
            {
                return await Task.Run(() => _tools[toolName](parameters ?? new Dictionary<string, object>()));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Debug] Failed to parse/execute tool call: {ex.Message}");
        }

        return null;
    }

    private Dictionary<string, object> ParseParameters(JsonElement paramsElement)
    {
        var parameters = new Dictionary<string, object>();
        
        foreach (var prop in paramsElement.EnumerateObject())
        {
            parameters[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString() ?? "",
                JsonValueKind.Number => prop.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => prop.Value.ToString()
            };
        }
        
        return parameters;
    }
}