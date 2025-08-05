using System.Text.Json.Serialization;

namespace McpServer.LLM;

public class McpRoot
{
    [JsonPropertyName("mcp")]
    public required Mcp Mcp { get; set; }
}

public class Mcp
{
    [JsonPropertyName("function")]
    public required McpFunction Function { get; set; }
}

public class McpFunction
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("parameters")]
    public required List<McpParameter> Parameters { get; set; }
}

public class McpParameter
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}