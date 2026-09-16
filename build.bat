@echo off
echo ===================================================
echo Compiling WinPods (WinUI 3 / .NET 8)
echo ===================================================

:: Check if dotnet is installed
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] dotnet CLI is not installed or not in PATH.
    echo Please install the .NET 8 SDK from https://dotnet.microsoft.com/download
    exit /b 1
)

:: Restore dependencies
echo [1/3] Restoring dependencies...
dotnet restore WinPods\WinPods.csproj

:: Build the project
echo [2/3] Building the project...
dotnet build WinPods\WinPods.csproj -c Release

:: Check build status
if %errorlevel% neq 0 (
    echo [ERROR] Build failed! Check the output above for errors.
    exit /b %errorlevel%
)

echo [3/3] Build successful!
echo.
echo You can run the application by executing the exe located in:
echo WinPods\bin\Release\net8.0-windows10.0.19041.0\win-x64\WinPods.exe
echo (Or similar depending on your architecture)
echo.
pause
