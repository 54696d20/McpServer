# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY *.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build the Blazor WASM app
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# Copy the published Blazor WASM files
COPY --from=build /app/publish/wwwroot .

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"] 