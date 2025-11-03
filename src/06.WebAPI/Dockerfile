# Use official .NET 9.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Use SDK image to build application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MyApp.WebAPI.csproj", "./"]

# Restore dependencies
RUN dotnet restore "MyApp.WebAPI.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "MyApp.WebAPI.csproj"  -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Environment variables for custom domain support
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "MyApp.WebAPI.dll"]