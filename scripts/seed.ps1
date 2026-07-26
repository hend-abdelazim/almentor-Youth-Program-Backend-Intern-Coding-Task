param(
    [switch]$Rebuild,
    [switch]$DevMode = $true
)
$ErrorActionPreference = "Stop"
$ApiProject = "src/TaskManagement.Api/TaskManagement.Api.csproj"
$InfraProject = "src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj"

if ($Rebuild) {
    Write-Host "Rebuilding project..." -ForegroundColor Cyan
    dotnet build $ApiProject -c Debug
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host ""
Write-Host "Applying EF Core migrations to the database..." -ForegroundColor Cyan
dotnet ef database update `
    --startup-project $ApiProject `
    --project $InfraProject `
    --context AppDbContext
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Migration command exited with code $LASTEXITCODE. This may be expected if DB already exists."
}

Write-Host ""
Write-Host "Starting API in Development mode to trigger seed data..." -ForegroundColor Green
Write-Host "Seed will run automatically when API starts in Development environment." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the server after seed completes." -ForegroundColor Yellow
Write-Host ""

$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project $ApiProject --launch-profile http
