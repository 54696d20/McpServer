using System.Text;
using McpServer.Client;
using McpServer.LLM;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add singleton services
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<LlmChat>(provider => 
    new LlmChat(Configuration.OllamaModelName, Configuration.OllamaBaseUrl));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

Console.WriteLine("🚀 MCP Task Management API Starting...");
Console.WriteLine($"🤖 Ollama Model: {Configuration.OllamaModelName}");
Console.WriteLine($"🔗 Ollama URL: {Configuration.OllamaBaseUrl}");
Console.WriteLine("✅ API is ready!");

app.Run();
