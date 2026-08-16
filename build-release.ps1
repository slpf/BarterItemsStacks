[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectPaths = @(
    Join-Path $PSScriptRoot "src\BarterItemsStacks\BarterItemsStacks.csproj"
    Join-Path $PSScriptRoot "src\BarterItemsStacksClient\BarterItemsStacksClient.csproj"
    Join-Path $PSScriptRoot "src\BarterItemsStacksFika\BarterItemsStacksFika.csproj"
)
$modInfoPath = Join-Path $PSScriptRoot "src\ModInfo.cs"
$buildPath = Join-Path $PSScriptRoot "build"
$bepInExPath = Join-Path $buildPath "BepInEx"
$sptRuntimePath = Join-Path $buildPath "SPT_Runtime"
$distributionPath = Join-Path $PSScriptRoot "distrib"

foreach ($projectPath in $projectPaths)
{
    if (!(Test-Path -LiteralPath $projectPath -PathType Leaf))
    {
        throw "Project file was not found: $projectPath"
    }
}

if (!(Test-Path -LiteralPath $modInfoPath -PathType Leaf))
{
    throw "ModInfo file was not found: $modInfoPath"
}

$modInfo = Get-Content -LiteralPath $modInfoPath -Raw
$versionMatch = [regex]::Match(
    $modInfo,
    'public\s+const\s+string\s+Version\s*=\s*"(?<version>[^"]+)"\s*;')

if (!$versionMatch.Success)
{
    throw "Unable to read the mod version from $modInfoPath"
}

$version = $versionMatch.Groups["version"].Value.Trim()

if ([string]::IsNullOrWhiteSpace($version) -or
    $version.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0)
{
    throw "Invalid mod version: '$version'"
}

Get-Command dotnet -ErrorAction Stop | Out-Null

foreach ($projectPath in $projectPaths)
{
    Write-Host "Building $projectPath"
    & dotnet build $projectPath --configuration Release

    if ($LASTEXITCODE -ne 0)
    {
        throw "Release build failed for $projectPath with exit code $LASTEXITCODE"
    }
}

if (!(Test-Path -LiteralPath $bepInExPath -PathType Container))
{
    throw "Build output was not found: $bepInExPath"
}

if (!(Test-Path -LiteralPath $sptRuntimePath -PathType Container))
{
    throw "Build output was not found: $sptRuntimePath"
}

New-Item -ItemType Directory -Path $distributionPath -Force | Out-Null

$archivePath = Join-Path $distributionPath "BarterItemsStacks-$version.zip"
$fikaArchivePath = Join-Path $distributionPath "BarterItemsStacksFika.zip"
$fikaDllPath = Join-Path $bepInExPath "plugins\BarterItemsStacksFika.dll"
$archiveBasePrefix = [System.IO.Path]::GetFullPath($buildPath).TrimEnd('\', '/') +
    [System.IO.Path]::DirectorySeparatorChar

if (!(Test-Path -LiteralPath $fikaDllPath -PathType Leaf))
{
    throw "Fika build output was not found: $fikaDllPath"
}

[System.IO.FileInfo[]]$files = @(
    Get-ChildItem -LiteralPath $bepInExPath, $sptRuntimePath -File -Recurse |
        Where-Object { $_.FullName -ne $fikaDllPath } |
        Sort-Object FullName
)

if ($files.Count -eq 0)
{
    throw "Build output contains no files for the main archive: $buildPath"
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function New-ReleaseArchive
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo[]]$Files
    )

    $archiveStream = [System.IO.File]::Open(
        $Path,
        [System.IO.FileMode]::Create,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)
    $archive = $null

    try
    {
        $archive = [System.IO.Compression.ZipArchive]::new(
            $archiveStream,
            [System.IO.Compression.ZipArchiveMode]::Create,
            $false)

        foreach ($file in $Files)
        {
            [string]$fullPath = [System.IO.Path]::GetFullPath($file.FullName)
            [string]$entryPath = $fullPath.Substring($archiveBasePrefix.Length).Replace('\', '/')

            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive,
                $fullPath,
                $entryPath,
                [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    }
    finally
    {
        if ($null -ne $archive)
        {
            $archive.Dispose()
        }

        $archiveStream.Dispose()
    }

    if (!(Test-Path -LiteralPath $Path -PathType Leaf))
    {
        throw "Archive was not created: $Path"
    }
}

New-ReleaseArchive -Path $archivePath -Files $files
New-ReleaseArchive -Path $fikaArchivePath -Files @(
    Get-Item -LiteralPath $fikaDllPath
)

Write-Host "Created $archivePath"
Write-Host "Created $fikaArchivePath"