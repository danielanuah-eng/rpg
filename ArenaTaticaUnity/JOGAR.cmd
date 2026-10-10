@echo off
cd /d "%~dp0"
if exist "Builds\Windows\ArenaTatica.exe" (
  start "" "Builds\Windows\ArenaTatica.exe"
) else (
  echo Execute a build Windows ou abra ABRIR-UNITY.cmd e pressione Play.
  pause
)
