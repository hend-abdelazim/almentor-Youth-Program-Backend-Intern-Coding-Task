param(
    [string]$MigrationName = $(throw "Migration name is required. Usage: .\scripts\migrate.ps1 -MigrationName MyMigration"),
    [switch]$Apply
)
$ErrorActionPreference = "Stop"
$ApiProject = "src/TaskManagement.Api/TaskManagement.Api.csproj"
$InfraProject = "src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj"
Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan
dotnet ef migrations add $MigrationName `
    --startup-project $ApiProject `
    --project $InfraProject `
    --context AppDbContext `
    --output-dir Persistence/Migrations
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if ($Apply) {
    Write-Host ""
    Write-Host "Applying migrations to database..." -ForegroundColor Cyan
    dotnet ef database update `
        --startup-project $ApiProject `
        --project $InfraProject `
        --context AppDbContext
}
exit $LASTEXITCODE
