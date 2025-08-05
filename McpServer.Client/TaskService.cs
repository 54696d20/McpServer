using System.Text.Json;

namespace McpServer.Client;

public class TaskService
{
    private readonly string _tasksFile;
    private List<TaskItem> _tasks = new();

    public TaskService()
    {
        // Use /app/data in container, or current directory for local development
        var dataDir = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
            ? "/app/data" 
            : Directory.GetCurrentDirectory();
        
        _tasksFile = Path.Combine(dataDir, "tasks.json");
        
        // Ensure data directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_tasksFile) ?? dataDir);
        
        LoadTasks();
    }

    public TaskResponse CreateTask(TaskItem task)
    {
        try
        {
            // Parse natural language dates
            if (task.DueDate == null && !string.IsNullOrEmpty(task.DueDate?.ToString()))
            {
                task.DueDate = ParseNaturalDate(task.DueDate.ToString());
            }
            
            _tasks.Add(task);
            SaveTasks();
            return new TaskResponse 
            { 
                Success = true, 
                Message = $"Task '{task.Title}' created successfully",
                Task = task
            };
        }
        catch (Exception ex)
        {
            return new TaskResponse 
            { 
                Success = false, 
                Message = $"Error creating task: {ex.Message}" 
            };
        }
    }

    public TaskResponse ReadTasks(string? filter = null)
    {
        try
        {
            var filteredTasks = filter?.ToLower() switch
            {
                "pending" => _tasks.Where(t => t.Status == "pending").ToList(),
                "completed" => _tasks.Where(t => t.Status == "completed").ToList(),
                "overdue" => _tasks.Where(t => t.Status == "pending" && t.DueDate < DateTime.Now).ToList(),
                _ => _tasks
            };

            return new TaskResponse 
            { 
                Success = true, 
                Message = $"Found {filteredTasks.Count} tasks",
                Tasks = filteredTasks
            };
        }
        catch (Exception ex)
        {
            return new TaskResponse 
            { 
                Success = false, 
                Message = $"Error reading tasks: {ex.Message}" 
            };
        }
    }

    public TaskResponse UpdateTask(string taskId, TaskItem updates)
    {
        try
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return new TaskResponse 
                { 
                    Success = false, 
                    Message = $"Task with ID {taskId} not found" 
                };
            }

            // Update properties
            if (!string.IsNullOrEmpty(updates.Title)) task.Title = updates.Title;
            if (updates.Description != null) task.Description = updates.Description;
            if (updates.DueDate != null) task.DueDate = updates.DueDate;
            if (!string.IsNullOrEmpty(updates.Priority)) task.Priority = updates.Priority;
            if (!string.IsNullOrEmpty(updates.Status)) 
            {
                task.Status = updates.Status;
                if (updates.Status == "completed" && task.CompletedAt == null)
                {
                    task.CompletedAt = DateTime.Now;
                }
            }

            SaveTasks();
            return new TaskResponse 
            { 
                Success = true, 
                Message = $"Task '{task.Title}' updated successfully",
                Task = task
            };
        }
        catch (Exception ex)
        {
            return new TaskResponse 
            { 
                Success = false, 
                Message = $"Error updating task: {ex.Message}" 
            };
        }
    }

    public TaskResponse DeleteTask(string taskId)
    {
        try
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return new TaskResponse 
                { 
                    Success = false, 
                    Message = $"Task with ID {taskId} not found" 
                };
            }

            _tasks.Remove(task);
            SaveTasks();
            return new TaskResponse 
            { 
                Success = true, 
                Message = $"Task '{task.Title}' deleted successfully" 
            };
        }
        catch (Exception ex)
        {
            return new TaskResponse 
            { 
                Success = false, 
                Message = $"Error deleting task: {ex.Message}" 
            };
        }
    }

    private DateTime? ParseNaturalDate(string dateString)
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
            _ => null
        };
    }

    private void LoadTasks()
    {
        try
        {
            if (File.Exists(_tasksFile))
            {
                var json = File.ReadAllText(_tasksFile);
                _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
        }
        catch
        {
            _tasks = new List<TaskItem>();
        }
    }

    private void SaveTasks()
    {
        try
        {
            var json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_tasksFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving tasks: {ex.Message}");
        }
    }
} 