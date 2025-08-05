namespace McpServer.Client;

public static class Configuration
{
    public static string OllamaModelName => 
        Environment.GetEnvironmentVariable("MCP_OLLAMA_MODEL") ?? "llama2:7b";
        
    public static string OllamaBaseUrl => 
        Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://localhost:11434";
        
    public static string McpServerPath => 
        Environment.GetEnvironmentVariable("MCP_SERVER_PATH") 
        ?? throw new InvalidOperationException("MCP_SERVER_PATH environment variable is required");
        
    public static string ClientName => "McpTaskClient";
    public static string ClientVersion => "1.0.0";
} 