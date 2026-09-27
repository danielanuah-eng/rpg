@echo off
title O Peso do Ceu - instalar o vault no Obsidian
setlocal
set "BASE=%~dp0"
set "DEST=%BASE%Anuah backup\Anuah backup"
set "WORK=%BASE%_publicar_peso"
set "LOG=%WORK%\log-vault.txt"
if not exist "%WORK%" mkdir "%WORK%"
if exist "%WORK%\pronto-vault.txt" del "%WORK%\pronto-vault.txt"
echo === O Peso do Ceu - instalando as notas no vault ===
echo instalando > "%LOG%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Force -LiteralPath '%BASE%vault.zip' -DestinationPath '%DEST%'" >> "%LOG%" 2>&1
echo CODIGO=%ERRORLEVEL% >> "%LOG%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "'total=' + (Get-ChildItem -LiteralPath '%DEST%' -Recurse -File | Measure-Object).Count" >> "%LOG%" 2>&1
echo OK > "%WORK%\pronto-vault.txt"
echo Pronto. Pode fechar.
timeout /t 15 >nul
