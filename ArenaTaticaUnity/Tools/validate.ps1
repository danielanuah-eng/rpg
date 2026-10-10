param([switch]$BuildWindows,[switch]$BuildWeb,[switch]$BuildAndroid)
$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
New-Item -ItemType Directory -Force (Join-Path $arenaRoot 'Artifacts') | Out-Null
& unity run $arenaRoot --editor-version 6000.6.4f1 -- -executeMethod ArenaBuild.Prepare -logFile (Join-Path $arenaRoot 'Artifacts/prepare.log')
if ($LASTEXITCODE -ne 0) { throw 'Preparação falhou' }
& unity test $arenaRoot --editor-version 6000.6.4f1 --mode EditMode --output (Join-Path $arenaRoot 'Artifacts/editmode.xml') --timeout 600
if ($LASTEXITCODE -ne 0) { throw 'Testes falharam' }
if ($BuildWindows) {
 & unity build $arenaRoot --editor-version 6000.6.4f1 --target StandaloneWindows64 --execute-method ArenaBuild.Windows --timeout 900 *> (Join-Path $arenaRoot 'Artifacts/windows-cli.log')
 if ($LASTEXITCODE -ne 0) { throw 'Build Windows falhou' }
}
if ($BuildWeb) {
 & unity build $arenaRoot --editor-version 6000.6.4f1 --target WebGL --execute-method ArenaBuild.Web --timeout 1800 *> (Join-Path $arenaRoot 'Artifacts/web-cli.log')
 if ($LASTEXITCODE -ne 0) { throw 'Build Web falhou' }
}
if ($BuildAndroid) {
 & (Join-Path $PSScriptRoot 'build-android.ps1')
}
