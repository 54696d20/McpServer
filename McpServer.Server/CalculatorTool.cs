using MCPSharp;

namespace McpServer.Server;

public class CalculatorTool
{
    [McpTool(name: "addition", Description = "This tool will add two numbers.")]
    public static int Addition(
        [McpParameter(required: true, description: "First number")] int firstNumber,
        [McpParameter(required: true, description: "Second number")] int secondNumber)
    {
        return firstNumber + secondNumber;
    }
}