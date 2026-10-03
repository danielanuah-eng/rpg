@echo off
rem Abre o app de testes do Arcanoclisma: liga o servidor local e abre o navegador.
cd /d "%~dp0\.."
start "Arcanoclisma - servidor do app de testes (deixe aberto)" node app\servidor.ts
timeout /t 3 /nobreak >nul
start "" http://localhost:8765
