param(
    [ValidateSet("unit", "integration", "all")]
    [string]$Type = "all",
    [string]$Configuration = "Debug",
    [switch]$NoBuild
)
$ErrorActionPreference = "Continue"

$UnitProject = "tests/TaskManagement.UnitTests/TaskManagement.UnitTests.csproj"
$IntegrationProject = "tests/TaskManagement.IntegrationTests/TaskManagement.IntegrationTests.csproj"

if (-not $NoBuild) {
    Write-Host "Building solution..." -ForegroundColor Cyan
    dotnet build TaskManagement.sln -c $Configuration
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host ""
}

$globalExitCode = 0

if ($Type -eq "unit" -or $Type -eq "all") {
    Write-Host "==================================" -ForegroundColor Magenta
    Write-Host " Running Unit Tests" -ForegroundColor Magenta
    Write-Host "==================================" -ForegroundColor Magenta
    dotnet test $UnitProject -c $Configuration --no-build -v normal
    if ($LASTEXITCODE -ne 0) { $globalExitCode = $LASTEXITCODE }
    Write-Host ""
}

if ($Type -eq "integration" -or $Type -eq "all") {
    Write-Host "==================================" -ForegroundColor Magenta
    Write-Host " Running Integration Tests" -ForegroundColor Magenta
    Write-Host "==================================" -ForegroundColor Magenta
    dotnet test $IntegrationProject -c $Configuration --no-build -v normal
    if ($LASTEXITCODE -ne 0) { $globalExitCode = $LASTEXITCODE }
    Write-Host ""
}

if ($globalExitCode -eq 0) {
    Write-Host "==================================" -ForegroundColor Green
    Write-Host " All tests PASSED!" -ForegroundColor Green
    Write-Host "==================================" -ForegroundColor Green
} else {
    Write-Host "==================================" -ForegroundColor Red
    Write-Host " Some tests FAILED!" -ForegroundColor Red
    Write-Host " Exit code: $globalExitCode" -ForegroundColor Red
    Write-Host "==================================" -ForegroundColor Red
}

exit $globalExitCode
