param([string]$Configuration = "Debug")
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore TaskManagement.sln
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host ""
Write-Host "Building solution ($Configuration)..." -ForegroundColor Cyan
dotnet build TaskManagement.sln -c $Configuration --no-restore
exit $LASTEXITCODE
