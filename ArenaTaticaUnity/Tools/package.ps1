param([ValidateSet('Windows','Web')][string[]]$Platforms=@('Windows','Web'), [switch]$SkipSource, [switch]$SkipAndroid)
$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
$builds = Join-Path $arenaRoot 'Builds'
New-Item -ItemType Directory -Force $builds | Out-Null
function New-ArenaArchive([string]$Destination,[string]$Directory,[string[]]$Names) {
    if (Test-Path -LiteralPath $Destination) { Remove-Item -LiteralPath $Destination -Force }
    # O mesmo tar.exe usado no build Windows evita entradas conflitantes recusadas pelo itch.io.
    & tar.exe -a -c -f $Destination --exclude 'UnityPlayerAccountSettings.asset*' -C $Directory @Names
    if ($LASTEXITCODE -ne 0) { throw "Falha ao gerar $Destination." }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($Destination)
    try {
        $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName.Replace('\','/').TrimEnd('/')
            if (-not $seen.Add($name)) { throw "Entrada duplicada no ZIP: $name" }
            if ($name.StartsWith('/') -or $name -match '(^|/)\.\.(/|$)') { throw "Caminho inválido no ZIP: $name" }
        }
    } finally { $archive.Dispose() }
}
$deliveries = @()
if (-not $SkipSource) {
    $source = @('Assets','Packages','ProjectSettings','Tools','Documentacao','LEIA-ME.md','VALIDACAO.md','ABRIR-UNITY.cmd','JOGAR.cmd','SERVIR-WEB.cmd','.gitignore')
    $sourceZip = Join-Path $builds 'ArenaTaticaUnity-Fonte.zip'
    New-ArenaArchive $sourceZip $arenaRoot $source
    $deliveries += Get-Item -LiteralPath $sourceZip
}
foreach ($platform in $Platforms) {
    $folder = Join-Path $builds $platform
    if (-not (Test-Path -LiteralPath $folder)) { continue }
    $files = @(Get-ChildItem -LiteralPath $folder | Where-Object { $_.Name -notmatch 'BackUpThisFolder|BurstDebugInformation' } | Select-Object -ExpandProperty Name)
    $platformZip = Join-Path $builds "ArenaTatica-$platform.zip"
    New-ArenaArchive $platformZip $folder $files
    $deliveries += Get-Item -LiteralPath $platformZip
}
$apk = Join-Path $builds 'Android/ArenaTatica.apk'
if (-not $SkipAndroid -and (Test-Path -LiteralPath $apk)) { $deliveries += Get-Item -LiteralPath $apk }
$manifest = $deliveries | ForEach-Object {
    [ordered]@{ file = $_.FullName.Substring($builds.Length+1); bytes = $_.Length; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $builds 'entregas.json') -Encoding utf8
Write-Output 'Pacotes e hashes gerados em Builds.'
