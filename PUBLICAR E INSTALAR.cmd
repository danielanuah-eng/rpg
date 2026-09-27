@echo off
title O Peso do Ceu - publicar e instalar
setlocal enabledelayedexpansion
set "BASE=%~dp0..\"
set "WORK=%BASE%_publicar_peso"
set "LOG=%WORK%\log.txt"
rem site novo no Netlify (criado em 24/09/2026): https://peso-do-ceu.netlify.app
set "SITE_ID=77a2ed2f-47c2-4259-8526-0cad6109cd14"
if not exist "%WORK%" mkdir "%WORK%"
cls
echo ===========================================================
echo   O PESO DO CEU - publicar o site e instalar as notas
echo ===========================================================
echo.
echo Pode deixar esta janela aberta. Leva uns 3 minutos
echo (na primeira vez um pouco mais: baixa as ferramentas).
echo.
echo NA PRIMEIRA VEZ o navegador abre uma pagina do Netlify
echo pedindo para autorizar: clique em "Authorize" e volte
echo para esta janela. Depois disso nao pede mais.
echo.
echo [1/4] preparando > "%LOG%"

rem ---------------------------------------------- Node portatil
rem O netlify-cli pede Node 22.13 ou mais novo. Baixa o Node 22 mais
rem recente (se a lista do site falhar, fica o 22.14.0) numa pasta propria.
rem O ForEach-Object abre a lista: no PowerShell 5 ela vinha inteira como
rem um item so, e a versao saia com todas as versoes juntas (erro 400).
set "NODE=%WORK%\node22"
if exist "%NODE%\node.exe" goto nodepronto
echo [1/4] baixando o Node.js ^(uma vez so, ~35 MB^)...
echo [1/4] baixando Node 22 >> "%LOG%"
if exist "%WORK%\tmpnode22" rmdir /s /q "%WORK%\tmpnode22"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ProgressPreference='SilentlyContinue'; [Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12; $z='%WORK%\node22.zip'; $v='v22.14.0'; try { $l=Invoke-RestMethod 'https://nodejs.org/dist/index.json'; $c=$l | ForEach-Object { $_ } | Where-Object { $_.version -like 'v22.*' } | Select-Object -First 1; if(($c.version -is [string]) -and ($c.version -like 'v22.*')){ $v=$c.version } } catch {}; 'versao: ' + $v; try { Invoke-WebRequest -UseBasicParsing -Uri ('https://nodejs.org/dist/' + $v + '/node-' + $v + '-win-x64.zip') -OutFile $z } catch { 'nao baixou ' + $v + ', tentando v22.14.0'; Invoke-WebRequest -UseBasicParsing -Uri 'https://nodejs.org/dist/v22.14.0/node-v22.14.0-win-x64.zip' -OutFile $z }; Expand-Archive -Force $z '%WORK%\tmpnode22'; $d=Get-ChildItem '%WORK%\tmpnode22' -Directory | Select-Object -First 1; if($d){ Move-Item -Force $d.FullName '%NODE%' }" >> "%LOG%" 2>&1
:nodepronto
if not exist "%NODE%\node.exe" (
  echo   ERRO: nao consegui preparar o Node.
  echo FALHA-NODE >> "%LOG%"
  goto fim
)
"%NODE%\node.exe" -v >> "%LOG%" 2>&1
echo [1/4] Node pronto.

rem ---------------------------------------------- notas no vault
echo [2/4] instalando as notas no Obsidian...
echo [2/4] vault >> "%LOG%"
rem Pastas com nome quebrado ("O Peso do C?u", "Refer?ncias") vieram de um
rem vault.zip antigo sem a marca de UTF-8. Nao apaga: move para Rpg\_lixo.
powershell -NoProfile -ExecutionPolicy Bypass -Command "$v='%BASE%Anuah backup\Anuah backup'; $l='%~dp0_lixo\nomes-quebrados'; foreach($n in @(('O Peso do C'+[char]0xC3+[char]0xA9+'u'),('Refer'+[char]0xC3+[char]0xAA+'ncias'))){ $p=Join-Path $v $n; if(Test-Path -LiteralPath $p){ New-Item -ItemType Directory -Force -Path $l | Out-Null; Move-Item -LiteralPath $p -Destination (Join-Path $l ($n+'-'+(Get-Date -Format yyyyMMddHHmmss))); 'movida: '+$n } }" >> "%LOG%" 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Force -LiteralPath '%BASE%vault.zip' -DestinationPath '%BASE%Anuah backup\Anuah backup'" >> "%LOG%" 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command "'notas no vault: ' + (Get-ChildItem -LiteralPath '%BASE%Anuah backup\Anuah backup' -Recurse -File).Count" >> "%LOG%" 2>&1
echo [2/4] Notas instaladas.

rem ---------------------------------------------- desempacotar o site
echo [3/4] desempacotando o site...
if exist "%WORK%\site" rmdir /s /q "%WORK%\site"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Force '%~dp0site.zip' '%WORK%\site'" >> "%LOG%" 2>&1
if not exist "%WORK%\site\index.html" (
  echo   ERRO: site.zip nao desempacotou.
  echo FALHA-ZIP >> "%LOG%"
  goto fim
)

rem ---------------------------------------------- publicar
rem Usa o login do Netlify guardado NESTE computador (netlify-cli).
rem Nada de credencial em arquivo: a autorizacao nao vence a cada dia.
echo [4/4] enviando para o Netlify...
echo [4/4] netlify >> "%LOG%"
set "PATH=%NODE%;%PATH%"
set "NETLIFY_TELEMETRY_DISABLED=1"
cd /d "%WORK%\site"
call "%NODE%\npx.cmd" -y netlify-cli@latest login
call "%NODE%\npx.cmd" -y netlify-cli@latest deploy --prod --dir . --site %SITE_ID% --message "O Peso do Ceu"
set ERRO=%ERRORLEVEL%
echo CODIGO=%ERRO% >> "%LOG%"
if "%ERRO%"=="0" (
  echo.
  echo   PRONTO. https://peso-do-ceu.netlify.app
  start "" "https://peso-do-ceu.netlify.app"
) else (
  echo.
  echo   O envio falhou ^(codigo %ERRO%^). Tire uma foto desta janela
  echo   e mande para o Claude.
)

:fim
echo.
echo O relatorio esta em _publicar_peso\log.txt
echo Esta janela fecha sozinha em 60 segundos.
timeout /t 60 >nul
