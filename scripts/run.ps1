param(
    [string]$Configuration = "Development",
    [string]$LaunchProfile = "http"
)
Write-Host "Building API project..." -ForegroundColor Cyan
dotnet build TaskManagement.sln -c Debug
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host ""
Write-Host "Running Task Management API..." -ForegroundColor Green
dotnet run --project src/TaskManagement.Api/TaskManagement.Api.csproj --launch-profile $LaunchProfile --no-build
