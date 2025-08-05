using Newtonsoft.Json;

namespace McpServer.Client;

public class TaskOperation
{
    [JsonProperty("operation")]
    public required string Operation { get; set; } // "create", "read", "update", "delete"
    
    [JsonProperty("task")]
    public TaskItem? Task { get; set; }
    
    [JsonProperty("taskId")]
    public string? TaskId { get; set; }
    
    [JsonProperty("filter")]
    public string? Filter { get; set; } // "all", "pending", "completed", "overdue"
}

public class TaskItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("title")]
    public required string Title { get; set; }
    
    [JsonProperty("description")]
    public string? Description { get; set; }
    
    [JsonProperty("dueDate")]
    [JsonConverter(typeof(StringDateConverter))]
    public string? DueDateString { get; set; } // Store as string from LLM
    
    [JsonIgnore]
    public DateTime? DueDate { get; set; } // Parsed date
    
    [JsonProperty("priority")]
    public string Priority { get; set; } = "medium"; // "low", "medium", "high"
    
    [JsonProperty("status")]
    public string Status { get; set; } = "pending"; // "pending", "in-progress", "completed"
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonProperty("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

public class TaskResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    
    [JsonProperty("message")]
    public string? Message { get; set; }
    
    [JsonProperty("tasks")]
    public List<TaskItem>? Tasks { get; set; }
    
    [JsonProperty("task")]
    public TaskItem? Task { get; set; }
}

public class StringDateConverter : JsonConverter<string?>
{
    public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;
        
        return reader.Value?.ToString();
    }

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteNull();
        else
            writer.WriteValue(value);
    }
} 