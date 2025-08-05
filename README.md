# MCP Task Management System

A complete **Model Context Protocol (MCP)** implementation with local LLM integration using .NET, MCPSharp, and Ollama. This system provides a conversational AI-powered task management application that runs entirely locally.

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Blazor WASM   │    │   Ollama LLM    │    │   Task Service  │
│   (Frontend)    │◄──►│   (Container)   │◄──►│   (Backend)     │
│   Port: 8080    │    │   Port: 11434   │    │   Port: 5000    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🚀 Quick Start with Docker (Recommended)

### Prerequisites
- **Docker Desktop** installed and running
- **Docker Compose** (usually included with Docker Desktop)
- **8GB+ RAM** (for LLM models)
- **10GB+ free disk space** (for models and containers)

### One-Command Setup

**Windows:**
```bash
start.bat
```

**macOS/Linux:**
```bash
./start.sh
```

**Manual:**
```bash
docker-compose up --build -d
```

### Access Your Application
- **🌐 Blazor UI**: http://localhost:8080
- **🔧 Task Service API**: http://localhost:5001
- **🤖 Ollama API**: http://localhost:11434

### First Run Notes
- **Model Download**: First run downloads the LLM model (~4GB)
- **Setup Time**: 5-10 minutes depending on internet speed
- **Monitor Progress**: `docker-compose logs -f ollama`

## 🛠️ Manual Setup (Alternative)

### Prerequisites
- **.NET 9.0 SDK**
- **Ollama** (for local LLM models)
- **Git**

### 1. Install Ollama
**macOS:**
```bash
brew install ollama
```

**Linux:**
```bash
curl -fsSL https://ollama.ai/install.sh | sh
```

**Windows:**
Download from [ollama.ai](https://ollama.ai)

### 2. Start Ollama and Pull a Model
```bash
# Start Ollama service
ollama serve

# In another terminal, pull a model
ollama pull llama2:7b
```

### 3. Clone and Build
```bash
git clone <your-repo>
cd McpServer
dotnet restore
dotnet build
```

### 4. Environment Configuration
Create a `.env` file based on `env.example`:
```bash
# Ollama model name (e.g., llama2:7b, mistral:7b, codellama:7b)
MCP_OLLAMA_MODEL=llama2:7b

# Path to your MCP server executable
MCP_SERVER_PATH=/path/to/McpServer.Server/bin/Debug/net9.0/McpServer.Server.exe
```

### 5. Set Environment Variables
```bash
export MCP_OLLAMA_MODEL="llama2:7b"
export MCP_SERVER_PATH="/path/to/McpServer.Server/bin/Debug/net9.0/McpServer.Server.exe"
```

### 6. Run the Application
```bash
# Start the MCP server
cd McpServer.Server
dotnet run

# In another terminal, run the client
cd McpServer.Client
dotnet run
```

## 🎯 How It Works

### Task Management Flow
1. **User Input**: Natural language task request
2. **LLM Processing**: Ollama receives the request and responds with structured JSON
3. **Task Operations**: System performs CRUD operations on tasks
4. **Response**: Results displayed to user

### Example Interactions
```
User: "Add a task to buy groceries tomorrow"
LLM: { "operation": "create", "task": { "title": "Buy groceries", "dueDate": "tomorrow" } }
Result: Task created successfully

User: "Show me all tasks"
LLM: { "operation": "read", "filter": "all" }
Result: Display all tasks

User: "Mark the grocery task as done"
LLM: { "operation": "update", "taskId": "123", "task": { "status": "completed" } }
Result: Task updated
```

## 📁 Project Structure

```
McpServer/
├── McpServer.LLM/           # LLM integration using Ollama HTTP API
├── McpServer.Client/        # Task management client and API
├── McpServer.Server/        # MCP server implementation
├── McpServer.UI/            # Blazor WASM UI application
├── docker-compose.yml       # Container orchestration
├── Dockerfile              # Task service container
├── start.sh                # Linux/macOS startup script
├── start.bat               # Windows startup script
└── README.md               # This file
```

## 🔧 Docker Commands

### Management
```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Restart services
docker-compose restart

# Rebuild and start
docker-compose up --build -d
```

### Individual Services
```bash
# View Ollama logs
docker-compose logs -f ollama

# View task service logs
docker-compose logs -f task-service

# View UI logs
docker-compose logs -f blazor-ui
```

## 🐛 Troubleshooting

### Docker Issues
1. **Docker not running**: Start Docker Desktop
2. **Port conflicts**: Check if ports 8080, 5000, or 11434 are in use
3. **Insufficient memory**: Increase Docker memory limit (8GB+ recommended)

### Ollama Issues
1. **Model not found**: `docker-compose logs ollama` to check download progress
2. **Connection refused**: Wait for Ollama to fully start
3. **Slow responses**: First run loads models into memory

### Application Issues
1. **UI not loading**: Check if Blazor container is running
2. **API errors**: Check task-service logs
3. **LLM errors**: Verify Ollama is healthy

### Using Different Models
```bash
# Pull different models
docker exec mcp-ollama ollama pull mistral:7b
docker exec mcp-ollama ollama pull codellama:7b

# Update environment variable
export MCP_OLLAMA_MODEL="mistral:7b"
docker-compose restart task-service
```

## 🚀 Benefits of This Approach

### Local & Private
- **No cloud dependencies**: Everything runs locally
- **Data privacy**: Your tasks never leave your machine
- **Offline capable**: Works without internet after setup

### Performance
- **Fast responses**: Local LLM inference
- **No API limits**: Unlimited usage
- **Customizable**: Use any Ollama model

### Developer Friendly
- **Easy setup**: One command with Docker
- **Cross-platform**: Works on Windows, Mac, Linux
- **Extensible**: Easy to add new features

## 🔮 Future Enhancements

- **More LLM Models**: Support for different model types
- **Advanced Task Features**: Recurring tasks, reminders, categories
- **User Authentication**: Multi-user support
- **Mobile App**: React Native or Flutter companion
- **API Extensions**: Webhook support, external integrations

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with Docker
5. Submit a pull request

---

**Happy task managing with AI! 🎯**

