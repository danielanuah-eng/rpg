param([ValidateSet('Prepare','Test','Windows')][string]$Action = 'Test')
$ErrorActionPreference = 'Stop'
$dungeonProject = Split-Path $PSScriptRoot -Parent
$dungeonArtifacts = Join-Path $dungeonProject 'Artifacts'
New-Item -ItemType Directory -Force -Path $dungeonArtifacts | Out-Null
if ($Action -eq 'Prepare' -or $Action -eq 'Test') {
    & unity run $dungeonProject --timeout 600 --no-tail --log-file (Join-Path $dungeonArtifacts 'dungeonbdx-prepare.log') -- -executeMethod DungeonBuild.Prepare
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao preparar DungeonBDX; ver Artifacts/dungeonbdx-prepare.log.' }
}
if ($Action -eq 'Test') {
    & unity test $dungeonProject --mode EditMode --output (Join-Path $dungeonArtifacts 'dungeonbdx-all-editmode.xml') --timeout 600
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes Unity; ver Artifacts/dungeonbdx-all-editmode.xml.' }
}
if ($Action -eq 'Windows') {
    & unity build $dungeonProject --target StandaloneWindows64 --execute-method DungeonBuild.Windows --timeout 1200 --format json
    if ($LASTEXITCODE -ne 0) { throw 'Falha no build DungeonBDX.' }
}
