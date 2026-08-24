[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-ValidatedBuildChildPath
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath,

        [Parameter(Mandatory = $true)]
        [string]$ChildPath
    )

    [string]$resolvedRootPath = [System.IO.Path]::GetFullPath($RootPath).TrimEnd('\', '/')
    [string]$resolvedChildPath = [System.IO.Path]::GetFullPath($ChildPath).TrimEnd('\', '/')
    [string]$rootPrefix = $resolvedRootPath + [System.IO.Path]::DirectorySeparatorChar

    if (Test-Path -LiteralPath $resolvedRootPath)
    {
        $rootItem = Get-Item -LiteralPath $resolvedRootPath -Force

        if (!$rootItem.PSIsContainer)
        {
            throw "Build path is not a directory: $resolvedRootPath"
        }

        if (($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)
        {
            throw "Build path must not be a reparse point: $resolvedRootPath"
        }
    }

    if (!$resolvedChildPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "Path is outside the build directory: $resolvedChildPath"
    }

    return $resolvedChildPath
}

$projectPaths = @(
    Join-Path $PSScriptRoot "src\BarterItemsStacks\BarterItemsStacks.csproj"
    Join-Path $PSScriptRoot "src\BarterItemsStacksClient\BarterItemsStacksClient.csproj"
)
$modInfoPath = Join-Path $PSScriptRoot "src\ModInfo.cs"
$buildPath = Join-Path $PSScriptRoot "build"
$bepInExPath = Get-ValidatedBuildChildPath -RootPath $buildPath -ChildPath (Join-Path $buildPath "BepInEx")
$sptRuntimePath = Get-ValidatedBuildChildPath -RootPath $buildPath -ChildPath (Join-Path $buildPath "SPT_Runtime")
$distributionPath = Join-Path $PSScriptRoot "distrib"
$itemsArchivePath = Join-Path $PSScriptRoot "src\BarterItemsStacks\Web\Assets\items\items.zip"

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

if (!(Test-Path -LiteralPath $itemsArchivePath -PathType Leaf))
{
    throw "Item images archive was not found: $itemsArchivePath"
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
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

foreach ($stagingPath in @($bepInExPath, $sptRuntimePath))
{
    if (Test-Path -LiteralPath $stagingPath)
    {
        $stagingItem = Get-Item -LiteralPath $stagingPath -Force

        if (!$stagingItem.PSIsContainer)
        {
            throw "Staging path is not a directory: $stagingPath"
        }

        if (($stagingItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)
        {
            throw "Staging path must not be a reparse point: $stagingPath"
        }

        [System.IO.Directory]::Delete($stagingPath, $true)
    }

    [System.IO.Directory]::CreateDirectory($stagingPath) | Out-Null
}

$temporaryPath = Join-Path ([System.IO.Path]::GetTempPath()) (
    "BarterItemsStacks-release-" + [System.Guid]::NewGuid().ToString("N"))
$temporaryItemsPath = Join-Path $temporaryPath "items"
$temporaryTargetsPath = Join-Path $temporaryPath "BarterItemsStacks.Release.targets"

try
{
    [System.IO.Directory]::CreateDirectory($temporaryItemsPath) | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory($itemsArchivePath, $temporaryItemsPath)

    [System.IO.FileInfo[]]$itemImages = @(
        Get-ChildItem -LiteralPath $temporaryItemsPath -Filter "*.webp" -File -Recurse
    )

    if ($itemImages.Count -eq 0)
    {
        throw "Item images archive contains no webp files: $itemsArchivePath"
    }

    [string]$targetsContent = @'
<Project>
  <ItemGroup>
    <EmbeddedResource Remove="Web/Assets/items/**/*.webp" />
    <EmbeddedResource Include="$(BarterItemsStacksItemAssetsPath)/**/*.webp">
      <LogicalName>BarterItemsStacks.Web.Assets.items.%(Filename)%(Extension)</LogicalName>
    </EmbeddedResource>
    <EmbeddedResource Include="Web/Assets/items/unknown.webp" Condition="Exists('Web/Assets/items/unknown.webp')">
      <LogicalName>BarterItemsStacks.Web.Assets.items.unknown.webp</LogicalName>
    </EmbeddedResource>
  </ItemGroup>
</Project>
'@
    [System.IO.File]::WriteAllText(
        $temporaryTargetsPath,
        $targetsContent,
        [System.Text.UTF8Encoding]::new($false))

    foreach ($projectPath in $projectPaths)
    {
        Write-Host "Building $projectPath"

        if ($projectPath -eq $projectPaths[0])
        {
            & dotnet build $projectPath --configuration Release `
                "-p:CustomAfterMicrosoftCommonTargets=$temporaryTargetsPath" `
                "-p:BarterItemsStacksItemAssetsPath=$temporaryItemsPath"
        }
        else
        {
            & dotnet build $projectPath --configuration Release
        }

        if ($LASTEXITCODE -ne 0)
        {
            throw "Release build failed for $projectPath with exit code $LASTEXITCODE"
        }
    }
}
finally
{
    if (Test-Path -LiteralPath $temporaryPath -PathType Container)
    {
        [System.IO.Directory]::Delete($temporaryPath, $true)
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
$archiveBasePrefix = [System.IO.Path]::GetFullPath($buildPath).TrimEnd('\', '/') +
    [System.IO.Path]::DirectorySeparatorChar

[System.IO.FileInfo[]]$files = @(
    Get-ChildItem -LiteralPath $bepInExPath, $sptRuntimePath -File -Recurse |
        Sort-Object FullName
)

if ($files.Count -eq 0)
{
    throw "Build output contains no files for the main archive: $buildPath"
}

function New-ReleaseArchive
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo[]]$Files
    )

    [string]$archiveId = [System.Guid]::NewGuid().ToString("N")
    [string]$temporaryArchivePath = $Path + "." + $archiveId + ".tmp"
    [string]$backupArchivePath = $Path + "." + $archiveId + ".bak"

    try
    {
        $archiveStream = [System.IO.File]::Open(
            $temporaryArchivePath,
            [System.IO.FileMode]::CreateNew,
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
                $entry = $archive.CreateEntry(
                    $entryPath,
                    [System.IO.Compression.CompressionLevel]::Optimal)
                $entry.LastWriteTime = [System.DateTimeOffset]::new(
                    2000,
                    1,
                    1,
                    0,
                    0,
                    0,
                    [System.TimeSpan]::Zero)
                $sourceStream = $null
                $entryStream = $null

                try
                {
                    $sourceStream = [System.IO.File]::OpenRead($fullPath)
                    $entryStream = $entry.Open()
                    $sourceStream.CopyTo($entryStream)
                }
                finally
                {
                    if ($null -ne $entryStream)
                    {
                        $entryStream.Dispose()
                    }

                    if ($null -ne $sourceStream)
                    {
                        $sourceStream.Dispose()
                    }
                }
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

        if ((Get-Item -LiteralPath $temporaryArchivePath).Length -le 0)
        {
            throw "Archive is empty: $temporaryArchivePath"
        }

        if (Test-Path -LiteralPath $Path -PathType Leaf)
        {
            [System.IO.File]::Replace($temporaryArchivePath, $Path, $backupArchivePath)
        }
        else
        {
            [System.IO.File]::Move($temporaryArchivePath, $Path)
        }
    }
    catch
    {
        if (!(Test-Path -LiteralPath $Path -PathType Leaf) -and
            (Test-Path -LiteralPath $backupArchivePath -PathType Leaf))
        {
            [System.IO.File]::Move($backupArchivePath, $Path)
        }

        throw
    }
    finally
    {
        if (Test-Path -LiteralPath $temporaryArchivePath -PathType Leaf)
        {
            [System.IO.File]::Delete($temporaryArchivePath)
        }

        if ((Test-Path -LiteralPath $Path -PathType Leaf) -and
            (Test-Path -LiteralPath $backupArchivePath -PathType Leaf))
        {
            [System.IO.File]::Delete($backupArchivePath)
        }
    }

    if (!(Test-Path -LiteralPath $Path -PathType Leaf))
    {
        throw "Archive was not created: $Path"
    }
}

New-ReleaseArchive -Path $archivePath -Files $files

Write-Host "Created $archivePath"
