$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
$builds = Join-Path $arenaRoot 'Builds'
New-Item -ItemType Directory -Force $builds | Out-Null
$source = @('Assets','Packages','ProjectSettings','Tools','Documentacao','LEIA-ME.md','VALIDACAO.md','ABRIR-UNITY.cmd','JOGAR.cmd','SERVIR-WEB.cmd','.gitignore') | ForEach-Object { Join-Path $arenaRoot $_ }
Compress-Archive -LiteralPath $source -DestinationPath (Join-Path $builds 'ArenaTaticaUnity-Fonte.zip') -Force
foreach ($platform in @('Windows','Web')) {
    $folder = Join-Path $builds $platform
    if (-not (Test-Path -LiteralPath $folder)) { continue }
    $files = Get-ChildItem -LiteralPath $folder | Where-Object { $_.Name -notmatch 'BackUpThisFolder|BurstDebugInformation' } | Select-Object -ExpandProperty FullName
    Compress-Archive -LiteralPath $files -DestinationPath (Join-Path $builds "ArenaTatica-$platform.zip") -Force
}
$deliveries = @(Get-ChildItem -LiteralPath $builds -Filter '*.zip')
$apk = Join-Path $builds 'Android/ArenaTatica.apk'
if (Test-Path -LiteralPath $apk) { $deliveries += Get-Item -LiteralPath $apk }
$manifest = $deliveries | ForEach-Object {
    [ordered]@{ file = $_.FullName.Substring($builds.Length+1); bytes = $_.Length; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $builds 'entregas.json') -Encoding utf8
Write-Output 'Pacotes e hashes gerados em Builds.'
