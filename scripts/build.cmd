@echo off
set SCRIPTS_DIR=%~dp0
set PROJECT_DIR=%SCRIPTS_DIR%..
dotnet publish "%PROJECT_DIR%\Ally.csproj" -r win-x64 -c Release
