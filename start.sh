#!/bin/bash

echo "🚀 Starting MCP Task Management System..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

echo "📦 Building and starting all services..."
docker-compose up --build -d

echo ""
echo "✅ Services started successfully!"
echo ""
echo "🌐 Access your application:"
echo "   • Blazor UI: http://localhost:8080"
echo "   • Task Service API: http://localhost:5001"
echo "   • Ollama API: http://localhost:11434"
echo ""
echo "📊 Monitor logs:"
echo "   • All services: docker-compose logs -f"
echo "   • Blazor UI: docker-compose logs -f blazor-ui"
echo "   • Task Service: docker-compose logs -f task-service"
echo "   • Ollama: docker-compose logs -f ollama"
echo ""
echo "🛑 To stop: docker-compose down" 