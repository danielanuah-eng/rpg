@echo off
setlocal
cd /d "%~dp0"
set "ARENA_GODOT=%~dp0.tools\godot\Godot_v4.6.1-stable_win64.exe"
if exist "%ARENA_GODOT%" (
    start "" "%ARENA_GODOT%" --path "%~dp0." --editor --language pt_BR
    exit /b 0
)
where godot >nul 2>nul
if not errorlevel 1 (
    start "" godot --path "%~dp0." --editor
    exit /b 0
)
echo Abra o Godot 4 e clique em Importar. Escolha project.godot nesta pasta.
echo O guia LEIA-ME.md explica os passos.
pause

