Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Compiling WinPods (WinUI 3 / .NET 8)" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

# Check if dotnet is installed
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] dotnet CLI is not installed or not in PATH." -ForegroundColor Red
    Write-Host "Please install the .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Restore dependencies
Write-Host "[1/3] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore WinPods\WinPods.csproj

# Build the project
Write-Host "[2/3] Building the project..." -ForegroundColor Yellow
dotnet build WinPods\WinPods.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed! Check the output above for errors." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "[3/3] Build successful!" -ForegroundColor Green
Write-Host ""
Write-Host "You can run the application by executing the exe located in:"
Write-Host "WinPods\bin\Release\net8.0-windows10.0.19041.0\win-x64\WinPods.exe" -ForegroundColor DarkGray
Write-Host "(Or similar depending on your architecture)"
Write-Host ""
