param([string]$StagePath)
# O Unity não gera Android em caminhos com acento ("Área de Trabalho"). O build roda numa cópia fixa em caminho ASCII,
# sincronizada a cada execução (robocopy /MIR de Assets, Packages e ProjectSettings). A Library da cópia é mantida,
# então só o primeiro build é completo (~48 min com IL2CPP); os seguintes levam ~18 min.
$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
if (-not $StagePath) { $StagePath = Join-Path $env:LOCALAPPDATA 'ArenaTaticaAndroid' }
$StagePath = [System.IO.Path]::GetFullPath($StagePath)
if ($StagePath -match '[^\x00-\x7F]') { throw 'StagePath precisa conter somente caracteres ASCII.' }
. (Join-Path $PSScriptRoot 'build-context.ps1')
$arenaBuildLock=Enter-ArenaBuild $arenaRoot $StagePath
try {
$arenaSourceCommit=(& git -C $arenaRoot rev-parse HEAD).Trim()
$artifacts = Join-Path $arenaRoot 'Artifacts'
New-Item -ItemType Directory -Force $artifacts | Out-Null
$log = Join-Path $artifacts 'android-cli.log'
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    Assert-ArenaChild $StagePath (Join-Path $StagePath $folder)
    & robocopy (Join-Path $arenaRoot $folder) (Join-Path $StagePath $folder) /MIR /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Falha ao sincronizar $folder (robocopy $LASTEXITCODE)." }
}
Set-Content -LiteralPath (Join-Path $artifacts 'android-stage.txt') -Value $StagePath
$apk = Join-Path $StagePath 'Builds/Android/ArenaTatica.apk'
Assert-ArenaChild $StagePath $apk
if (Test-Path -LiteralPath $apk) { Remove-Item -LiteralPath $apk -Force }
Write-Output "Cópia Android sincronizada: $StagePath"
# O servidor de compilação C# (VBCSCompiler) é compartilhado entre editores; o do editor aberto no projeto com acento
# não lê os .rsp da cópia (CS2011). Encerrá-lo faz o build subir um servidor próprio; o editor cria outro quando precisar.
Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" | Where-Object { $_.CommandLine -match 'VBCSCompiler' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
# No PowerShell 5.1, stderr de programa externo com 'Stop' vira erro fatal e abortava o script no meio do build.
$ErrorActionPreference = 'Continue'
& unity build $StagePath --editor-version 6000.6.4f1 --target Android --execute-method ArenaBuild.Android --timeout 5400 --format json *> $log
$ErrorActionPreference = 'Stop'
if ($LASTEXITCODE -ne 0) { throw 'Build Android falhou; consultar Artifacts/android-cli.log.' }
if (-not (Test-Path -LiteralPath $apk)) { throw 'O Unity terminou sem gerar o APK; consultar Artifacts/android-cli.log.' }
$destination = Join-Path $arenaRoot 'Builds/Android'
New-Item -ItemType Directory -Force $destination | Out-Null
Copy-Item -LiteralPath $apk -Destination (Join-Path $destination 'ArenaTatica.apk') -Force
Save-ArenaBuildReceipt $arenaRoot $StagePath 'Android' $arenaSourceCommit
Write-Output "APK pronto: $destination\ArenaTatica.apk"
} finally { $arenaBuildLock.Dispose() }
