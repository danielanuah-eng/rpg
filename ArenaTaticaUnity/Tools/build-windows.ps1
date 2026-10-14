param([string]$StagePath,[switch]$SkipCacheSeed)
# Versão de PC sem fechar o editor: o projeto aberto fica travado, então o build roda numa cópia própria (como o Android),
# por padrão a mesma cópia do Android (uma segunda Library tem ~16 GB e lotou o disco C). Saída em Builds/Windows e o zip
# em Builds/ArenaTatica-Windows.zip.
$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
if (-not $StagePath) { $StagePath = Join-Path $env:LOCALAPPDATA 'ArenaTaticaAndroid' }
$StagePath = [System.IO.Path]::GetFullPath($StagePath)
. (Join-Path $PSScriptRoot 'build-context.ps1')
$arenaBuildLock=Enter-ArenaBuild $arenaRoot $StagePath
try {
$arenaSourceCommit=(& git -C $arenaRoot rev-parse HEAD).Trim()
$artifacts = Join-Path $arenaRoot 'Artifacts'
New-Item -ItemType Directory -Force $artifacts | Out-Null
$log = Join-Path $artifacts 'windows-cli.log'
$seed = Join-Path $env:LOCALAPPDATA 'ArenaTaticaAndroid/Library'
if (-not (Test-Path -LiteralPath $seed)) { $seed=Join-Path $arenaRoot 'Library' }
if (-not $SkipCacheSeed -and -not (Test-Path (Join-Path $StagePath 'Library')) -and (Test-Path $seed)) {
    Assert-ArenaChild $StagePath (Join-Path $StagePath 'Library')
    & robocopy $seed (Join-Path $StagePath 'Library') /MIR /MT:8 /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -lt 0 -or $LASTEXITCODE -ge 8) { throw "Falha ao copiar cache (robocopy $LASTEXITCODE)." }
}
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    Assert-ArenaChild $StagePath (Join-Path $StagePath $folder)
    & robocopy (Join-Path $arenaRoot $folder) (Join-Path $StagePath $folder) /MIR /MT:8 /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
    if ($LASTEXITCODE -lt 0 -or $LASTEXITCODE -ge 8) { throw "Falha ao sincronizar $folder (robocopy $LASTEXITCODE)." }
}
$out = Join-Path $StagePath 'Builds/Windows'
Assert-ArenaChild $StagePath $out
if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Recurse -Force }
Write-Output "Cópia Windows sincronizada: $StagePath"
# Mesmo cuidado do build Android: o servidor de compilação do editor aberto não lê os .rsp da cópia (CS2011).
Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" | Where-Object { $_.CommandLine -match 'VBCSCompiler' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
$ErrorActionPreference = 'Continue'
& unity build $StagePath --editor-version 6000.6.4f1 --target StandaloneWindows64 --execute-method ArenaBuild.Windows --timeout 5400 --format json *> $log
$ErrorActionPreference = 'Stop'
if ($LASTEXITCODE -ne 0) { throw 'Build Windows falhou; consultar Artifacts/windows-cli.log.' }
if (-not (Test-Path -LiteralPath (Join-Path $out 'ArenaTatica.exe'))) { throw 'O Unity terminou sem gerar o .exe; consultar Artifacts/windows-cli.log.' }
$destination = Join-Path $arenaRoot 'Builds/Windows'
Assert-ArenaChild $arenaRoot $destination
& robocopy $out $destination /MIR /MT:8 /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
if ($LASTEXITCODE -lt 0 -or $LASTEXITCODE -ge 8) { throw "Falha ao copiar o build (robocopy $LASTEXITCODE)." }
Save-ArenaBuildReceipt $arenaRoot $StagePath 'Windows' $arenaSourceCommit
& (Join-Path $PSScriptRoot 'package.ps1') -Platforms Windows -SkipSource -SkipAndroid
Write-Output "Versão de PC pronta: $(Join-Path $arenaRoot 'Builds/ArenaTatica-Windows.zip')"
} finally { $arenaBuildLock.Dispose() }
