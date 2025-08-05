namespace McpServer.Client.UI.Client.Models;

public class TaskItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TaskResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<TaskItem>? Tasks { get; set; }
}

public class ChatResponse
{
    public bool Success { get; set; }
    public string? Response { get; set; }
    public string? Message { get; set; }
} 