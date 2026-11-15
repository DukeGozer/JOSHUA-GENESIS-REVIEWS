$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'dist\JoshuaSpike5Genesis-win-x64'
New-Item -ItemType Directory -Force $out | Out-Null
dotnet publish (Join-Path $root 'src\Genesis.Host\Genesis.Host.csproj') -c Release -r win-x64 --self-contained false -o $out
dotnet publish (Join-Path $root 'src\Genesis.UI\Genesis.UI.csproj') -c Release -r win-x64 --self-contained false -o $out
Write-Host "Build fertig: $out"
Write-Host 'Start: Genesis.UI.exe (Host wird bei Bedarf gestartet).'