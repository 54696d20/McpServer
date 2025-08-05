using McpServer.Server;
using MCPSharp;

internal class Program
{
    static async Task Main(string[] args)
    {
        MCPServer.Register<CalculatorTool>();

        await MCPServer.StartAsync(
            serverName: "SimpleMcp.Server",
            version: "v1.0.0");
    }
}