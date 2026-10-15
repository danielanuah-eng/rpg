@echo off
cd /d "%~dp0"
if not exist "Builds\DungeonBDX\DungeonBDX.exe" (
  echo Build ausente. Execute no PowerShell: .\Tools\dungeonbdx.ps1 -Action Windows
  pause
  exit /b 1
)
start "DungeonBDX" "Builds\DungeonBDX\DungeonBDX.exe"
