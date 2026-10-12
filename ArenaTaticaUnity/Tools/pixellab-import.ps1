# Baixa um personagem do PixelLab e grava em Resources/Sprites/<Pasta>:
#   <direção>.png          quadro parado (rotação)
#   <clipe>_<direção>.png  tira horizontal de quadros quadrados (animação)
# Uso: powershell -File Tools/pixellab-import.ps1 -Id <uuid> -Pasta Ciantia [-Estado Idle] [-Prefixo ""]
# -Prefixo renomeia os clipes de um segundo estado (ex.: "berserk" grava berserkidle_south-east.png).
param([Parameter(Mandatory)][string]$Id,[Parameter(Mandatory)][string]$Pasta,[string]$Estado="",[string]$Prefixo="",[switch]$SemRotacoes)
$ErrorActionPreference="Stop"
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$raiz=Split-Path -Parent $PSScriptRoot
$destino=Join-Path $raiz "Assets\ArenaTatica\Resources\Sprites\$Pasta"
New-Item -ItemType Directory -Force $destino | Out-Null
$zipPath=Join-Path $env:TEMP "pixellab-$Id.zip"
Invoke-WebRequest -UseBasicParsing "https://api.pixellab.ai/mcp/characters/$Id/download" -OutFile $zipPath
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
function Abrir($nome){ $e=$zip.GetEntry($nome); if(!$e){ throw "Faltou no zip: $nome" }; $s=$e.Open(); $m=New-Object IO.MemoryStream; $s.CopyTo($m); $s.Dispose(); [Drawing.Image]::FromStream($m) }
try {
  $meta=(New-Object IO.StreamReader($zip.GetEntry("metadata.json").Open())).ReadToEnd() | ConvertFrom-Json
  $estados=@($meta.states); if($Estado){ $estados=@($estados | Where-Object { $_.folder -eq $Estado }) }
  $st=$estados[0]; if(!$st){ throw "Estado não encontrado: $Estado" }
  if(!$SemRotacoes){
    foreach($p in $st.frames.rotations.PSObject.Properties){ $img=Abrir $p.Value; $img.Save((Join-Path $destino "$($p.Name).png"),[Drawing.Imaging.ImageFormat]::Png); $img.Dispose() }
  }
  $n=0
  if($st.frames.animations){
    foreach($clip in $st.frames.animations.PSObject.Properties){
      # Grupo extra do mesmo clipe (direção reenviada) vem como "<clipe>-<8 hex>": junta ao principal.
      $base=$clip.Name -replace '-[0-9a-f]{8}$',''
      $extra=$base -ne $clip.Name
      $nome=($Prefixo+$base) -replace '[^A-Za-z0-9]',''
      foreach($dir in $clip.Value.PSObject.Properties){
        if($extra -and $st.frames.animations.$base.($dir.Name)){ continue }
        $quadros=@($dir.Value | ForEach-Object { Abrir $_ })
        $lado=($quadros | ForEach-Object { [Math]::Max($_.Width,$_.Height) } | Measure-Object -Maximum).Maximum
        $tira=New-Object Drawing.Bitmap ($lado*$quadros.Count),$lado,([Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g=[Drawing.Graphics]::FromImage($tira); $g.InterpolationMode='NearestNeighbor'; $g.PixelOffsetMode='Half'
        for($i=0;$i -lt $quadros.Count;$i++){ $q=$quadros[$i]; $g.DrawImage($q,(New-Object Drawing.Rectangle ($i*$lado+[int](($lado-$q.Width)/2)),([int](($lado-$q.Height)/2)),$q.Width,$q.Height)); $q.Dispose() }
        $g.Dispose(); $tira.Save((Join-Path $destino "$($nome)_$($dir.Name).png"),[Drawing.Imaging.ImageFormat]::Png); $tira.Dispose(); $n++
      }
    }
  }
  "OK $Pasta ($($st.folder)): $n tiras"
} finally { $zip.Dispose() }
