param(
    [ValidateSet('Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$src = Join-Path $root 'ICE/bin/x64/Release'
$dst = Join-Path $root 'ICE-latest'
$zip = Join-Path $root 'ICE-latest.zip'
$sln = Join-Path $root 'ICE.sln'

if (-not $env:DALAMUD_HOME) {
    $env:DALAMUD_HOME = Join-Path $env:APPDATA 'XIVLauncherCN/addon/Hooks/dev'
}

Write-Host "Building ICE ($Configuration x64)..."
dotnet build $sln -c $Configuration -p:Platform=x64
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item (Join-Path $src 'ICE.dll'), (Join-Path $src 'ICE.json'), (Join-Path $src 'OtterGui.dll') $dst -Force

$exclude = @(
    'Dalamud.dll',
    'FFXIVClientStructs.dll',
    'InteropGenerator.Runtime.dll',
    'Lumina.dll',
    'Lumina.Excel.dll',
    'ImGui.NET.dll',
    'ImGuiScene.dll'
)
Get-ChildItem $src -Filter *.dll |
    Where-Object { $exclude -notcontains $_.Name } |
    ForEach-Object { Copy-Item $_.FullName $dst -Force }

$dll = Join-Path $dst 'ICE.dll'
$version = [System.Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString()

if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $dst '*') -DestinationPath $zip -CompressionLevel Optimal

$zipDll = Join-Path $env:TEMP 'ice-dll-zip-check.dll'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zip)
$entry = $archive.GetEntry('ICE.dll')
if (-not $entry) { throw 'ICE.dll missing from ICE-latest.zip' }
[System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $zipDll, $true)
$archive.Dispose()
$zipVersion = [System.Reflection.AssemblyName]::GetAssemblyName($zipDll).Version.ToString()
Remove-Item $zipDll -Force

if ($version -ne $zipVersion) {
    throw "Zip version mismatch: folder=$version zip=$zipVersion"
}

Write-Host "Version:      $version"
Write-Host "Output folder: $dst"
Write-Host "Output zip:    $zip"
