$ErrorActionPreference = 'Stop'
$phase8Root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$phase8Godot = Join-Path $phase8Root '.tools/godot/Godot_v4.6.1-stable_win64_console.exe'
$phase8Host = Start-Process -FilePath $phase8Godot -ArgumentList '--headless --path . --log-file phase8-host.log --script res://tests/test_phase8_network.gd -- --host' -WorkingDirectory $phase8Root -WindowStyle Hidden -PassThru
$phase8Client = Start-Process -FilePath $phase8Godot -ArgumentList '--headless --path . --log-file phase8-client.log --script res://tests/test_phase8_network.gd -- --client' -WorkingDirectory $phase8Root -WindowStyle Hidden -PassThru
if (-not $phase8Host.WaitForExit(45000)) { $phase8Host.Kill(); throw 'Timeout do Host' }
if (-not $phase8Client.WaitForExit(5000)) { $phase8Client.Kill(); throw 'Timeout do cliente' }
Get-Content -LiteralPath (Join-Path $phase8Root 'phase8-host.log')
Get-Content -LiteralPath (Join-Path $phase8Root 'phase8-client.log')
if ($phase8Host.ExitCode -ne 0 -or $phase8Client.ExitCode -ne 0) { exit 1 }
