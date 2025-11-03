# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file dan restore dependencies
COPY ["MyApp.BlazorUI.csproj", "./"]
RUN dotnet restore "./MyApp.BlazorUI.csproj"

# Copy seluruh source code
COPY . .

# Build project
RUN dotnet build "MyApp.BlazorUI.csproj" -c Release -o /app/build

# Publish hasil build
RUN dotnet publish "MyApp.BlazorUI.csproj" -c Release -o /app/publish /p:UseAppHost=false\\

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Environment
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "MyApp.BlazorUI.dll"]
