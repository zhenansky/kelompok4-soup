FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/08.BlazorUI/MyApp.BlazorUI.csproj", "src/08.BlazorUI/"]
RUN dotnet restore "src/08.BlazorUI/MyApp.BlazorUI.csproj"
COPY . .
WORKDIR "/src/src/08.BlazorUI"
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