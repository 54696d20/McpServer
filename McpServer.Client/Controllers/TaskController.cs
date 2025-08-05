using Microsoft.AspNetCore.Mvc;
using McpServer.Client;
using McpServer.LLM;
using Newtonsoft.Json;
using System.Text;

namespace McpServer.Client.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;
    private readonly LlmChat _llmChat;

    public TaskController()
    {
        _taskService = new TaskService();
        _llmChat = new LlmChat(Configuration.OllamaModelName, Configuration.OllamaBaseUrl);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            Console.WriteLine($"Received chat request: {request.Message}");
            
            // Check if the message is task-related
            bool isTaskRelated = IsTaskRelatedMessage(request.Message);
            
            string fullResponse = "";
            await foreach (string response in _llmChat.SendAsync(request.Message, useTaskMode: isTaskRelated))
            {
                fullResponse += response;
            }

            Console.WriteLine($"LLM Response: {fullResponse}");

            if (isTaskRelated)
            {
                // Handle task operations
                return await HandleTaskOperation(fullResponse);
            }
            else
            {
                // Handle general conversation
                return Ok(new ChatResponse 
                { 
                    Success = true, 
                    Response = fullResponse.Trim(),
                    Message = "Conversation response"
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing chat request: {ex.Message}");
            // Instead of returning an error, return a friendly message
            return Ok(new ChatResponse 
            { 
                Success = true, 
                Response = "I'm having trouble processing that right now. Could you try rephrasing your request?",
                Message = "Error occurred"
            });
        }
    }

    private bool IsTaskRelatedMessage(string message)
    {
        var lowerMessage = message.ToLower().Trim();
        
        // First, check for common conversational phrases that should NOT be treated as task-related
        var conversationalPhrases = new[]
        {
            "hello", "hi", "hey", "good morning", "good afternoon", "good evening",
            "how are you", "how's it going", "what's up", "nice to meet you",
            "thank you", "thanks", "bye", "goodbye", "see you", "talk to you later",
            "what time", "what day", "what's the weather", "tell me a joke",
            "who are you", "what can you do", "help", "?"
        };
        
        if (conversationalPhrases.Any(phrase => lowerMessage.Contains(phrase)))
        {
            return false;
        }
        
        // Check for specific task operation patterns (more specific)
        var taskOperationPatterns = new[]
        {
            @"\b(add|create|new)\s+(a\s+)?task\b",
            @"\b(show|list|display|get)\s+(my\s+)?tasks?\b",
            @"\b(update|edit|modify)\s+(a\s+)?task\b",
            @"\b(delete|remove)\s+(a\s+)?task\b",
            @"\b(complete|finish|mark)\s+(a\s+)?task\b",
            @"\b(due|priority|status)\s+(high|low|medium|urgent|important)\b",
            @"\b(todo|to-do|to do)\b",
            @"\b(reminder|schedule|deadline)\b"
        };
        
        var hasTaskPatterns = taskOperationPatterns.Any(pattern => 
            System.Text.RegularExpressions.Regex.IsMatch(lowerMessage, pattern));
        
        // Also check for task-related keywords in context
        var taskKeywords = new[]
        {
            "task", "tasks", "todo", "reminder", "schedule", "deadline"
        };
        
        var hasTaskKeywords = taskKeywords.Any(keyword => lowerMessage.Contains(keyword));
        
        return hasTaskPatterns || hasTaskKeywords;
    }

    private async Task<IActionResult> HandleTaskOperation(string fullResponse)
    {
        Console.WriteLine($"Full LLM Response: {fullResponse}");
        
        // Parse the response
        string? json = ExtractFirstJsonObject(fullResponse);
        if (string.IsNullOrEmpty(json))
        {
            Console.WriteLine("No JSON found in response, treating as general conversation");
            return Ok(new ChatResponse 
            { 
                Success = true, 
                Response = fullResponse.Trim(),
                Message = "LLM response (no task operation detected)"
            });
        }

        // Fix incomplete JSON
        var braceCount = json.Count(c => c == '{') - json.Count(c => c == '}');
        if (braceCount > 0)
        {
            json += new string('}', braceCount);
        }

        Console.WriteLine($"Parsed JSON: {json}");

        // Try to deserialize with error handling
        TaskOperation? taskOp = null;
        try
        {
            taskOp = JsonConvert.DeserializeObject<TaskOperation>(json);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            Console.WriteLine($"Problematic JSON: {json}");
            
            // Try to fix common JSON issues
            json = FixCommonJsonIssues(json);
            Console.WriteLine($"Attempting to fix JSON: {json}");
            
            try
            {
                taskOp = JsonConvert.DeserializeObject<TaskOperation>(json);
            }
            catch (JsonException ex2)
            {
                Console.WriteLine($"Still failed after fixing: {ex2.Message}");
                // Instead of returning an error, treat as general conversation
                return Ok(new ChatResponse 
                { 
                    Success = true, 
                    Response = fullResponse.Trim(),
                    Message = "LLM response (JSON parsing failed)"
                });
            }
        }
        
        if (taskOp == null)
        {
            Console.WriteLine("TaskOperation is null, treating as general conversation");
            return Ok(new ChatResponse 
            { 
                Success = true, 
                Response = fullResponse.Trim(),
                Message = "LLM response (no task operation)"
            });
        }

        TaskResponse result = taskOp.Operation switch
        {
            "create" => await HandleCreateTask(taskOp),
            "read" => await HandleReadTasks(taskOp),
            "update" => await HandleUpdateTask(taskOp),
            "delete" => await HandleDeleteTask(taskOp),
            _ => new TaskResponse { Success = false, Message = $"Unknown operation: {taskOp.Operation}" }
        };

        return Ok(result);
    }

    [HttpGet("tasks")]
    public IActionResult GetTasks([FromQuery] string? filter = null)
    {
        try
        {
            var result = _taskService.ReadTasks(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("tasks")]
    public IActionResult CreateTask([FromBody] TaskItem task)
    {
        try
        {
            var result = _taskService.CreateTask(task);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("tasks/{id}")]
    public IActionResult UpdateTask(string id, [FromBody] TaskItem updates)
    {
        try
        {
            var result = _taskService.UpdateTask(id, updates);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("tasks/{id}")]
    public IActionResult DeleteTask(string id)
    {
        try
        {
            var result = _taskService.DeleteTask(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<TaskResponse> HandleCreateTask(TaskOperation taskOp)
    {
        if (taskOp.Task == null)
        {
            return new TaskResponse { Success = false, Message = "No task data provided" };
        }

        // Parse natural language dates
        if (!string.IsNullOrEmpty(taskOp.Task.DueDateString))
        {
            taskOp.Task.DueDate = ParseNaturalDate(taskOp.Task.DueDateString);
        }

        return _taskService.CreateTask(taskOp.Task);
    }

    private async Task<TaskResponse> HandleReadTasks(TaskOperation taskOp)
    {
        return _taskService.ReadTasks(taskOp.Filter);
    }

    private async Task<TaskResponse> HandleUpdateTask(TaskOperation taskOp)
    {
        if (string.IsNullOrEmpty(taskOp.TaskId))
        {
            return new TaskResponse { Success = false, Message = "Task ID is required for updates" };
        }

        if (taskOp.Task == null)
        {
            return new TaskResponse { Success = false, Message = "No update data provided" };
        }

        return _taskService.UpdateTask(taskOp.TaskId, taskOp.Task);
    }

    private async Task<TaskResponse> HandleDeleteTask(TaskOperation taskOp)
    {
        if (string.IsNullOrEmpty(taskOp.TaskId))
        {
            return new TaskResponse { Success = false, Message = "Task ID is required for deletion" };
        }

        return _taskService.DeleteTask(taskOp.TaskId);
    }

    private static DateTime? ParseNaturalDate(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;

        var lowerDate = dateString.ToLower().Trim();
        
        return lowerDate switch
        {
            "today" => DateTime.Today,
            "tomorrow" => DateTime.Today.AddDays(1),
            "next week" => DateTime.Today.AddDays(7),
            "next month" => DateTime.Today.AddMonths(1),
            "this friday" => GetNextDayOfWeek(DayOfWeek.Friday),
            "next friday" => GetNextDayOfWeek(DayOfWeek.Friday).AddDays(7),
            "this monday" => GetNextDayOfWeek(DayOfWeek.Monday),
            "next monday" => GetNextDayOfWeek(DayOfWeek.Monday).AddDays(7),
            "this tuesday" => GetNextDayOfWeek(DayOfWeek.Tuesday),
            "next tuesday" => GetNextDayOfWeek(DayOfWeek.Tuesday).AddDays(7),
            "this wednesday" => GetNextDayOfWeek(DayOfWeek.Wednesday),
            "next wednesday" => GetNextDayOfWeek(DayOfWeek.Wednesday).AddDays(7),
            "this thursday" => GetNextDayOfWeek(DayOfWeek.Thursday),
            "next thursday" => GetNextDayOfWeek(DayOfWeek.Thursday).AddDays(7),
            "this saturday" => GetNextDayOfWeek(DayOfWeek.Saturday),
            "next saturday" => GetNextDayOfWeek(DayOfWeek.Saturday).AddDays(7),
            "this sunday" => GetNextDayOfWeek(DayOfWeek.Sunday),
            "next sunday" => GetNextDayOfWeek(DayOfWeek.Sunday).AddDays(7),
            _ => null
        };
    }

    private static DateTime GetNextDayOfWeek(DayOfWeek dayOfWeek)
    {
        var today = DateTime.Today;
        var daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(daysUntilTarget);
    }

    private static string? ExtractFirstJsonObject(string text)
    {
        int start = text.IndexOf('{');
        if (start == -1) return null;

        int depth = 0;
        bool inString = false;
        StringBuilder sb = new StringBuilder();

        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"' && (i == 0 || text[i - 1] != '\\'))
                inString = !inString;

            if (!inString)
            {
                if (c == '{') depth++;
                else if (c == '}') depth--;
            }

            sb.Append(c);

            if (depth == 0 && !inString)
                break;
        }

        return sb.ToString();
    }

    private static string FixCommonJsonIssues(string json)
    {
        // Fix common LLM JSON issues
        var fixedJson = json;
        
        // Remove any trailing commas before closing braces/brackets
        fixedJson = System.Text.RegularExpressions.Regex.Replace(fixedJson, @",(\s*[}\]])", "$1");
        
        // Fix unescaped quotes in strings
        fixedJson = System.Text.RegularExpressions.Regex.Replace(fixedJson, @"([^\\])""([^""\\]*?)""([^""\\]*?)""", "$1\"$2\\\"$3\"");
        
        // Fix missing quotes around property names
        fixedJson = System.Text.RegularExpressions.Regex.Replace(fixedJson, @"([{,])\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*:", "$1 \"$2\":");
        
        // Fix missing quotes around string values
        fixedJson = System.Text.RegularExpressions.Regex.Replace(fixedJson, @":\s*([a-zA-Z][a-zA-Z0-9\s]*?)([,}])", ": \"$1\"$2");
        
        // Remove any control characters that might cause parsing issues
        fixedJson = new string(fixedJson.Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());
        
        return fixedJson;
    }
}

public class ChatRequest
{
    public string Message { get; set; } = "";
}

public class ChatResponse
{
    public bool Success { get; set; }
    public string? Response { get; set; }
    public string? Message { get; set; }
} 