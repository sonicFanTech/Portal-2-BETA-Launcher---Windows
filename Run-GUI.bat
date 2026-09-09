@echo off
setlocal
cd /d "%~dp0"
dotnet run --project "%~dp0Portal2BetaLauncher\Portal2BetaLauncher.csproj" -c Release -- --gui
pause
