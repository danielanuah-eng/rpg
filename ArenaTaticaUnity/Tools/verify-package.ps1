param([switch]$Source)
$ErrorActionPreference='Stop'
$arenaRoot=Split-Path $PSScriptRoot -Parent
$archivePath=Join-Path $arenaRoot $(if($Source){'Builds/ArenaTaticaUnity-Fonte.zip'}else{'Builds/ArenaTatica-Windows.zip'})
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive=[IO.Compression.ZipFile]::OpenRead($archivePath)
$count=0
$extract=Join-Path $arenaRoot ('Artifacts/019-extracted-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $extract | Out-Null
$prefix=[IO.Path]::GetFullPath($extract).TrimEnd('\')+'\'
$names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
try {
    foreach($entry in $archive.Entries) {
        $name=$entry.FullName.Replace('\','/')
        if(-not $names.Add($name.TrimEnd('/')) -or $name.StartsWith('/') -or $name.Contains(':') -or $name -match '(^|/)\.\.(/|$)') {throw 'ZIP contém duplicata ou caminho inválido.'}
        if($name -match '(^|/)UnityPlayerAccountSettings\.asset') {throw 'ZIP contém configuração local de conta Unity.'}
        if($name.EndsWith('/')) {continue}
        $dest=[IO.Path]::GetFullPath((Join-Path $extract $name))
        if(-not $dest.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)) {throw 'Extração fora do destino.'}
        New-Item -ItemType Directory -Force (Split-Path $dest -Parent) | Out-Null
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$dest)
        $original=Join-Path $(if($Source){$arenaRoot}else{Join-Path $arenaRoot 'Builds/Windows'}) $name
        if(-not (Test-Path -LiteralPath $original) -or (Get-FileHash -LiteralPath $dest).Hash -ne (Get-FileHash -LiteralPath $original).Hash) {throw "Conteúdo extraído difere da origem: $name"}
        $count++
    }
} finally {$archive.Dispose()}
if(-not $Source -and -not (Test-Path -LiteralPath (Join-Path $extract 'ArenaTatica.exe'))) {throw 'Executável não está na raiz do pacote Windows.'}
$report=[ordered]@{file=[IO.Path]::GetFileName($archivePath);completedAt=[DateTime]::UtcNow.ToString('o');sha256=(Get-FileHash -LiteralPath $archivePath).Hash;files=$count;duplicateEntries=0;extractedFilesMatch=$true;accountFiles=0;extractedPath=$extract}
$report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $arenaRoot $(if($Source){'Artifacts/019-source-package.json'}else{'Artifacts/019-windows-package.json'})) -Encoding utf8
$report | ConvertTo-Json
