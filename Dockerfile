FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY McpServer.Core.sln ./
COPY McpServer.LLM/*.csproj ./McpServer.LLM/
COPY McpServer.Client/*.csproj ./McpServer.Client/
COPY McpServer.Server/*.csproj ./McpServer.Server/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build the task service (we'll use the Client project as our backend API)
WORKDIR /src/McpServer.Client
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Default command
ENTRYPOINT ["dotnet", "McpServer.Client.dll"] 