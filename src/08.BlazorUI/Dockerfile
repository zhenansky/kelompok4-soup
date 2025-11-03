FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MyApp.BlazorUI.csproj", "./"]
RUN dotnet restore "MyApp.BlazorUI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MyApp.BlazorUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.BlazorUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Custom domain configuration
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "MyApp.BlazorUI.dll"]