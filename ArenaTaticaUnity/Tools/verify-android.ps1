param(
    [string]$ExpectedVersion = '1.2.2',
    [int]$ExpectedVersionCode = 5,
    [string]$ExpectedSourceCommit,
    [string]$ExpectedCertificateSha256 = 'eab099f58dcbbf1a63d863228ecefe22ace342749f895d5f646f477dc5f27425',
    [string]$EditorRoot = 'C:/Program Files/Unity/Hub/Editor/6000.6.4f1/Editor'
)
$ErrorActionPreference = 'Stop'
$arenaRoot = Split-Path $PSScriptRoot -Parent
$apk = Join-Path $arenaRoot 'Builds/Android/ArenaTatica.apk'
$receiptPath = Join-Path $arenaRoot 'Builds/Android/proveniencia.json'
if (-not (Test-Path -LiteralPath $apk)) { throw 'APK atual ausente.' }
$androidTools = Join-Path $EditorRoot 'Data/PlaybackEngines/AndroidPlayer'
$buildTools = Get-ChildItem -LiteralPath (Join-Path $androidTools 'SDK/build-tools') -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+\.\d+$' } |
    Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (-not $buildTools) { throw 'SDK Android sem build-tools.' }
$aapt = Join-Path $buildTools.FullName 'aapt.exe'
$java = Join-Path $androidTools 'OpenJDK/bin/java.exe'
$signer = Join-Path $buildTools.FullName 'lib/apksigner.jar'
# aapt não aceita o caminho absoluto com acento; o argumento relativo funciona.
Push-Location -LiteralPath $arenaRoot
try {
$badging = @(& $aapt dump badging 'Builds/Android/ArenaTatica.apk')
if ($LASTEXITCODE -ne 0) { throw 'Não foi possível ler o manifesto do APK.' }
$package = [regex]::Match(($badging -join "`n"), "(?m)^package: name='([^']+)' versionCode='(\d+)' versionName='([^']+)'")
if (-not $package.Success -or $package.Groups[1].Value -ne 'org.arcanoclisma.arenatatica' -or
    $package.Groups[2].Value -ne [string]$ExpectedVersionCode -or $package.Groups[3].Value -ne $ExpectedVersion) {
    throw "APK não corresponde a org.arcanoclisma.arenatatica $ExpectedVersion/code $ExpectedVersionCode. Não publicar build antiga."
}
$native = @($badging | Where-Object { $_ -match '^native-code:' })
if ($native.Count -ne 1 -or $native[0].Trim() -ne "native-code: 'arm64-v8a'") { throw 'APK precisa conter somente ARM64.' }
if (-not ($badging -match "^uses-permission: name='android.permission.INTERNET'")) { throw 'APK sem permissão de internet.' }
$signature = @(& $java -jar $signer verify --print-certs 'Builds/Android/ArenaTatica.apk')
if ($LASTEXITCODE -ne 0) { throw 'Assinatura do APK inválida.' }
$certificate = [regex]::Match(($signature -join "`n"), '(?m)^Signer #1 certificate SHA-256 digest: ([0-9a-f]{64})\s*$')
if (-not $certificate.Success -or $certificate.Groups[1].Value -ne $ExpectedCertificateSha256.ToLowerInvariant()) {
    throw 'Certificado diferente da versão Android anterior; atualização deve preservar a assinatura.'
}
if (-not (Test-Path -LiteralPath $receiptPath)) { throw 'Recibo do build Android ausente.' }
$receipt = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
if (-not $ExpectedSourceCommit) {
    $ExpectedSourceCommit = (& git -C $arenaRoot rev-parse HEAD).Trim()
}
if ($receipt.platform -ne 'Android' -or $receipt.version -ne $ExpectedVersion -or $receipt.protocol -ne 6 -or
    $receipt.sourceCommit -ne $ExpectedSourceCommit -or $receipt.codeDirty -ne $false -or $receipt.stagedCodeMatches -ne $true) {
    throw 'Recibo Android não corresponde ao código final testado.'
}
$report = [ordered]@{
    file = 'ArenaTatica.apk'
    completedAt = [DateTime]::UtcNow.ToString('o')
    sourceCommit = $receipt.sourceCommit
    version = $package.Groups[3].Value
    versionCode = [int]$package.Groups[2].Value
    package = $package.Groups[1].Value
    architecture = 'arm64-v8a'
    internetPermission = $true
    signatureVerified = $true
    certificateSha256 = $certificate.Groups[1].Value
    certificateMatchesPrevious = $true
    bytes = (Get-Item -LiteralPath $apk).Length
    sha256 = (Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash.ToLowerInvariant()
    physicalDeviceTested = $false
}
$report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $arenaRoot 'Artifacts/019-android-package.json') -Encoding utf8
$report | ConvertTo-Json
} finally { Pop-Location }
