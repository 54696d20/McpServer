using System.Text;
using System.Text.Json;

namespace McpServer.LLM;

public class LlmChat : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly string _baseUrl;

    public LlmChat(string modelName = "llama2:7b", string? baseUrl = null)
    {
        _modelName = modelName;
        _baseUrl = baseUrl ?? "http://localhost:11434";
        _httpClient = new HttpClient();
    }

    public async IAsyncEnumerable<string> SendAsync(string userMessage, bool useTaskMode = false)
    {
        string prompt;
        
        if (useTaskMode)
        {
            // Task management mode
            prompt = $@"You are a task management assistant. When users ask about tasks, respond with ONLY valid JSON in this exact format:

{{
  ""operation"": ""[create|read|update|delete]"",
  ""task"": {{
    ""title"": ""[task title]"",
    ""description"": ""[optional description or null]"",
    ""dueDate"": ""[date string or null]"",
    ""priority"": ""[low|medium|high]"",
    ""status"": ""[pending|in-progress|completed]""
  }},
  ""taskId"": ""[task id for update/delete operations or null]"",
  ""filter"": ""[all|pending|completed|overdue for read operations or null]""
}}

IMPORTANT RULES:
1. Respond with ONLY the JSON object, no other text
2. Use double quotes for all strings
3. Use null for optional fields that are not provided
4. Do not include trailing commas
5. Ensure all strings are properly quoted

Examples:
- ""Add a task to buy groceries tomorrow"" → {{ ""operation"": ""create"", ""task"": {{ ""title"": ""Buy groceries"", ""dueDate"": ""tomorrow"", ""priority"": ""medium"", ""status"": ""pending"", ""description"": null }}, ""taskId"": null, ""filter"": null }}
- ""Show me all tasks"" → {{ ""operation"": ""read"", ""task"": null, ""taskId"": null, ""filter"": ""all"" }}
- ""Mark task 123 as completed"" → {{ ""operation"": ""update"", ""task"": {{ ""title"": null, ""description"": null, ""dueDate"": null, ""priority"": null, ""status"": ""completed"" }}, ""taskId"": ""123"", ""filter"": null }}
- ""Delete task 456"" → {{ ""operation"": ""delete"", ""task"": null, ""taskId"": ""456"", ""filter"": null }}

User request: {userMessage}

Respond with ONLY the JSON:";
        }
        else
        {
            // Simple conversational mode
            prompt = $@"You are a helpful AI assistant. Respond naturally and conversationally to the user's message.

User: {userMessage}
Assistant:";
        }

        var request = new
        {
            model = _modelName,
            prompt = prompt,
            stream = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!string.IsNullOrEmpty(line))
            {
                JsonDocument? jsonDoc = null;
                try
                {
                    jsonDoc = JsonDocument.Parse(line);
                }
                catch (JsonException)
                {
                    continue;
                }

                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("response", out var responseElement))
                {
                    var responseValue = responseElement.GetString();
                    if (!string.IsNullOrEmpty(responseValue))
                    {
                        yield return responseValue;
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    private class OllamaResponse
    {
        public string? Response { get; set; }
        public bool Done { get; set; }
    }
}