@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo Building Portal2BetaLauncher...

where dotnet.exe >nul 2>&1
if %errorlevel%==0 (
    dotnet build "%~dp0Portal2BetaLauncher.sln" --configuration Release --property:Platform=x64
    exit /b %errorlevel%
)

set "VSWHERE=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist "%VSWHERE%" (
    for /f "usebackq delims=" %%I in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
        if not defined MSBUILD set "MSBUILD=%%I"
    )
)

if defined MSBUILD (
    "%MSBUILD%" "%~dp0Portal2BetaLauncher.sln" /m /p:Configuration=Release /p:Platform=x64
    exit /b %errorlevel%
)

echo.
echo ERROR: Could not find the .NET SDK or Visual Studio MSBuild.
echo Open the solution in Visual Studio 2026, or install the .NET 8 SDK / MSBuild workload.
exit /b 1
