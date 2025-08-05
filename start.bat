@echo off
echo 🚀 Starting MCP Task Management System with Docker...
echo ==================================================

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not running. Please start Docker Desktop and try again.
    pause
    exit /b 1
)

REM Check if docker-compose is available
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ docker-compose is not installed. Please install Docker Compose and try again.
    pause
    exit /b 1
)

echo 📦 Building and starting containers...
docker-compose up --build -d

echo ⏳ Waiting for services to start...
timeout /t 10 /nobreak >nul

echo 🔍 Checking service status...
docker-compose ps

echo.
echo ✅ Services are starting up!
echo.
echo 🌐 Access your application:
echo    - Blazor UI: http://localhost:8080
echo    - Task Service API: http://localhost:5001
echo    - Ollama API: http://localhost:11434
echo.
echo 📋 Useful commands:
echo    - View logs: docker-compose logs -f
echo    - Stop services: docker-compose down
echo    - Restart services: docker-compose restart
echo.
echo 🎯 First time setup:
echo    - The first run will download the LLM model (several GB)
echo    - This may take 5-10 minutes depending on your internet
echo    - You can monitor progress with: docker-compose logs -f ollama

pause 