# SonarQube Analysis Script for Windows PowerShell
# Run this with:  .\sonar-scan.ps1

Write-Host "???? Starting SonarQube Analysis for MyApp..." -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

# Configuration
$PROJECT_KEY = "kelompok4-soup"
$PROJECT_NAME = "kelompok4-soup"
$SONAR_HOST = "http://localhost:9000"
$SONAR_TOKEN = "sqa_238883e03d389ef23f732f7b56e0a6210b63505e"

# Check if SonarQube is running
Write-Host ""
Write-Host "???? Checking SonarQube server..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$SONAR_HOST/api/system/status" -UseBasicParsing -ErrorAction Stop
    Write-Host "??? SonarQube server is running" -ForegroundColor Green
}
catch {
    Write-Host "??? SonarQube server is not accessible at $SONAR_HOST" -ForegroundColor Red
    Write-Host "Please start SonarQube manually or via Docker." -ForegroundColor Yellow
    exit 1
}

# Check if dotnet-sonarscanner is installed
Write-Host ""
Write-Host "???? Checking dotnet-sonarscanner..." -ForegroundColor Yellow
$tools = dotnet tool list -g
if ($tools -match "dotnet-sonarscanner") {
    Write-Host "??? dotnet-sonarscanner is installed" -ForegroundColor Green
}
else {
    Write-Host "??? dotnet-sonarscanner not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}

# Clean previous build
Write-Host ""
Write-Host "???? Cleaning previous build..." -ForegroundColor Yellow
dotnet clean MyApp.sln

# Begin SonarQube analysis
Write-Host ""
Write-Host "???? Starting SonarQube scanner..." -ForegroundColor Cyan
dotnet sonarscanner begin `
    "/k:$PROJECT_KEY" `
    "/n:$PROJECT_NAME" `
    "/d:sonar.host.url=$SONAR_HOST" `
    "/d:sonar.login=$SONAR_TOKEN" `
    "/d:sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml" `
    "/d:sonar.cs.vstest.reportsPaths=**/*.trx" `
    "/d:sonar.coverage.exclusions=**/Migrations/**,**/wwwroot/**,**/*.cshtml,**/Program.cs" `
    "/d:sonar.exclusions=**/wwwroot/**,**/obj/**,**/bin/**"

if ($LASTEXITCODE -ne 0) {
    Write-Host "??? Failed to start SonarQube scanner" -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host ""
Write-Host "???? Building project..." -ForegroundColor Yellow
dotnet build MyApp.sln --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "??? Build failed" -ForegroundColor Red
    exit 1
}

# Run tests with coverage
Write-Host ""
Write-Host "???? Running tests with coverage..." -ForegroundColor Yellow
dotnet test MyApp.sln `
    --configuration Release `
    --no-build `
    --logger "trx" `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=opencover `
    /p:CoverletOutput=./TestResults/

# End SonarQube analysis
Write-Host ""
Write-Host "???? Uploading results to SonarQube..." -ForegroundColor Cyan
dotnet sonarscanner end "/d:sonar.login=$SONAR_TOKEN"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "??? SonarQube analysis completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "???? View results at: $SONAR_HOST/dashboard?id=$PROJECT_KEY" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "???? Tips:" -ForegroundColor Yellow
    Write-Host "  - Check 'Issues' tab for code smells, bugs, and vulnerabilities"
    Write-Host "  - Review 'Security Hotspots' for potential security issues"
    Write-Host "  - Monitor Coverage to see test coverage metrics"
    Write-Host ""
}
else {
    Write-Host "??? SonarQube analysis failed" -ForegroundColor Red
    exit 1
}

