@echo off
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File "%~dp0Tools\dungeonbdx-claude.ps1"
if errorlevel 1 pause
