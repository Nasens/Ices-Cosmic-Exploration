param(
    [ValidateSet('PrintVersion', 'Show', 'BumpTranslation', 'BumpSync', 'SetMain')]
    [string]$Mode = 'Show',
    [string]$MainVersion,
    [string]$JsonPath = (Join-Path $PSScriptRoot '..\fork-version.json')
)

function Get-ForkVersionData {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        throw "Version file not found: $Path"
    }
    return Get-Content -Raw -Path $Path | ConvertFrom-Json
}

function Get-MainSegments {
    param([string]$Version)
    $parts = $Version.Split('.')
    if ($parts.Count -ne 4) {
        throw "Main version must be 4 segments (e.g. 0.0.78.15), got: $Version"
    }
    return [int[]]@(
        [int]$parts[0],
        [int]$parts[1],
        [int]$parts[2],
        [int]$parts[3]
    )
}

function Get-CombinedVersion {
    param($Data)
    $main = Get-MainSegments $Data.main
    $sync = [int]$Data.sync
    $translation = [int]$Data.translation
    return "0.0.$($main[2] + $sync).$($main[3] + $translation)"
}

function Save-ForkVersionData {
    param(
        [string]$Path,
        $Data
    )
    $output = [ordered]@{
        main        = [string]$Data.main
        sync        = [int]$Data.sync
        translation = [int]$Data.translation
    }
    ($output | ConvertTo-Json -Depth 3) + [Environment]::NewLine | Set-Content -Path $Path -Encoding UTF8 -NoNewline
}

$data = Get-ForkVersionData $JsonPath

switch ($Mode) {
    'SetMain' {
        if ([string]::IsNullOrWhiteSpace($MainVersion)) {
            throw 'SetMain requires -MainVersion (e.g. 0.0.78.15).'
        }
        [void](Get-MainSegments $MainVersion)
        $data.main = $MainVersion
        Save-ForkVersionData $JsonPath $data
    }
    'BumpSync' {
        $data.sync = [int]$data.sync + 1
        Save-ForkVersionData $JsonPath $data
    }
    'BumpTranslation' {
        $data.translation = [int]$data.translation + 1
        Save-ForkVersionData $JsonPath $data
    }
}

$combined = Get-CombinedVersion $data

switch ($Mode) {
    'PrintVersion' { Write-Output $combined }
    default {
        Write-Output "main:        $($data.main)"
        Write-Output "fork:        0.$($data.sync).$($data.translation)"
        Write-Output "combined:    $combined"
    }
}
