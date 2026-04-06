# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution and project files
COPY ["GestionDeStock.sln", "."]
COPY ["GestionDeStock.API/GestionDeStock.API.csproj", "GestionDeStock.API/"]

# Restore dependencies
RUN dotnet restore "GestionDeStock.API/GestionDeStock.API.csproj"

# Copy the entire source code
COPY . .

# Build the application
RUN dotnet build "GestionDeStock.API/GestionDeStock.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GestionDeStock.API/GestionDeStock.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD dotnet --version || exit 1

# Run the application
ENTRYPOINT ["dotnet", "GestionDeStock.API.dll"]
