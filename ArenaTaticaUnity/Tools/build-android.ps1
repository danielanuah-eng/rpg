param([string]$StagePath)
$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
if (-not $StagePath) { $StagePath = Join-Path ([System.IO.Path]::GetTempPath()) ('ArenaTaticaUnity-Android-' + [guid]::NewGuid().ToString('N')) }
$StagePath = [System.IO.Path]::GetFullPath($StagePath)
if ($StagePath -match '[^\x00-\x7F]') { throw 'StagePath precisa conter somente caracteres ASCII.' }
if (Test-Path -LiteralPath $StagePath) { throw 'Escolha uma pasta temporária nova para evitar misturar projetos.' }
New-Item -ItemType Directory -Path $StagePath | Out-Null
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $arenaRoot $folder) -Destination (Join-Path $StagePath $folder) -Recurse
}
$artifacts = Join-Path $arenaRoot 'Artifacts'
New-Item -ItemType Directory -Force $artifacts | Out-Null
Set-Content -LiteralPath (Join-Path $artifacts 'android-stage.txt') -Value $StagePath
Write-Output "Cópia temporária Android: $StagePath"
& unity build $StagePath --editor-version 6000.6.4f1 --target Android --execute-method ArenaBuild.Android --timeout 5400 --format json *> (Join-Path $artifacts 'android-cli.log')
if ($LASTEXITCODE -ne 0) { throw 'Build Android falhou; consultar Artifacts/android-cli.log.' }
$destination = Join-Path $arenaRoot 'Builds/Android'
New-Item -ItemType Directory -Force $destination | Out-Null
Copy-Item -LiteralPath (Join-Path $StagePath 'Builds/Android/ArenaTatica.apk') -Destination (Join-Path $destination 'ArenaTatica.apk')
Write-Output "APK pronto: $destination/ArenaTatica.apk"
