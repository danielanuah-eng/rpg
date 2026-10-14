# Helpers compartilhados: uma só cópia de build, caminhos conferidos e identidade do código.
function Enter-ArenaBuild([string]$ArenaRoot,[string]$StagePath) {
    $root=[IO.Path]::GetFullPath($ArenaRoot).TrimEnd('\','/')
    $stage=[IO.Path]::GetFullPath($StagePath).TrimEnd('\','/')
    if ($stage -eq [IO.Path]::GetPathRoot($stage).TrimEnd('\') -or $root -eq $stage -or $root.StartsWith($stage+'\',[StringComparison]::OrdinalIgnoreCase) -or $stage.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)) {
        throw 'A cópia de build precisa estar numa pasta própria, fora do projeto de origem.'
    }
    New-Item -ItemType Directory -Force $stage | Out-Null
    if ((Get-Item -LiteralPath $stage).LinkType) { throw 'A cópia de build não pode ser um link de diretório.' }
    $active=Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.IndexOf($stage,[StringComparison]::OrdinalIgnoreCase) -ge 0 }
    if ($active) { throw 'A cópia já está aberta no Unity; aguarde o build/teste existente.' }
    try { return [IO.File]::Open((Join-Path $stage '.arena-build.lock'),[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None) }
    catch { throw 'Outro build da Arena usa esta cópia. Windows e Android devem ser sequenciais.' }
}
function Assert-ArenaChild([string]$Parent,[string]$Path) {
    $prefix=[IO.Path]::GetFullPath($Parent).TrimEnd('\','/')+'\'
    if (-not [IO.Path]::GetFullPath($Path).StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)) { throw "Destino fora da pasta de build: $Path" }
}
function Save-ArenaBuildReceipt([string]$ArenaRoot,[string]$StagePath,[string]$Platform,[string]$SourceCommit) {
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $ArenaRoot 'Assets') -Filter '*.cs' -File -Recurse) {
        $staged=Join-Path $StagePath $file.FullName.Substring($ArenaRoot.Length+1)
        if (-not (Test-Path -LiteralPath $staged) -or (Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $staged).Hash) {
            throw 'O código mudou durante o build; não empacotar uma revisão diferente da testada.'
        }
    }
    $source=@(foreach($directory in @('Assets','Packages','ProjectSettings')) {
        Get-ChildItem -LiteralPath (Join-Path $ArenaRoot $directory) -File -Recurse | Where-Object { $_.Name -notlike 'UnityPlayerAccountSettings.asset*' }
    })
    $lines=@($source | Sort-Object FullName | ForEach-Object { (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash+' '+$_.FullName.Substring($ArenaRoot.Length+1).Replace('\','/') })
    $hasher=[Security.Cryptography.SHA256]::Create()
    try { $digest=[BitConverter]::ToString($hasher.ComputeHash([Text.Encoding]::UTF8.GetBytes(($lines -join "`n")))).Replace('-','').ToLowerInvariant() } finally { $hasher.Dispose() }
    $revision=if($SourceCommit) { $SourceCommit } else { (& git -C $ArenaRoot rev-parse HEAD).Trim() }
    & git -C $ArenaRoot diff --quiet -- Assets/ArenaTatica/Scripts Assets/Editor
    $codeDirty=$LASTEXITCODE -ne 0
    $receipt=[ordered]@{version='1.2.1';protocol=5;platform=$Platform;completedAt=[DateTime]::UtcNow.ToString('o');sourceCommit=$revision;codeDirty=$codeDirty;stagedCodeMatches=$true;sourceTreeSha256=$digest;sourceFiles=$source.Count;stage=$StagePath}
    $destination=Join-Path $ArenaRoot "Builds/$Platform/proveniencia.json"
    $receipt | ConvertTo-Json | Set-Content -LiteralPath $destination -Encoding utf8
    $lines | Set-Content -LiteralPath (Join-Path $ArenaRoot "Artifacts/020-inputs-$Platform.sha256") -Encoding utf8
}
