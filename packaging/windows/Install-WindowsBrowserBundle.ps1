param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Archive')]
    [string]$SourceArchivePath,
    [Parameter(Mandatory = $true, ParameterSetName = 'Directory')]
    [string]$SourceDirectoryPath,
    [string]$DestinationRoot = (Join-Path $env:LOCALAPPDATA 'ClawLinks'),
    [Parameter(Mandatory = $true)]
    [string]$BundleVersion,
    [string]$Platform = 'windows-x64',
    [string]$Architecture = 'x64',
    [string]$UpstreamProductLine = 'stable',
    [string]$SourceRevision = 'unknown',
    [string]$ReleaseChannel = 'staging',
    [string]$DisplayName = 'Claw Browser',
    [string]$EntryExecutableRelativePath = 'firefox.exe'
)

$ErrorActionPreference = 'Stop'

function Copy-DirectoryContents {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,
        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory
    )

    $items = Get-ChildItem -LiteralPath $SourceDirectory -Force
    foreach ($item in $items)
    {
        Copy-Item -LiteralPath $item.FullName -Destination $DestinationDirectory -Recurse -Force
    }
}

function Resolve-ExtractedBundleRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExtractRoot
    )

    $entries = Get-ChildItem -LiteralPath $ExtractRoot -Force
    $directories = @($entries | Where-Object { $_.PSIsContainer })
    $files = @($entries | Where-Object { -not $_.PSIsContainer })

    if ($files.Count -eq 0 -and $directories.Count -eq 1)
    {
        return $directories[0].FullName
    }

    return $ExtractRoot
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
$browserRoot = Join-Path $DestinationRoot 'browser'
$bundleRoot = Join-Path $browserRoot (Join-Path 'bundles' $BundleVersion)
$policySource = Join-Path $repoRoot 'config\firefox\policies.json'
$localSettingsSource = Join-Path $repoRoot 'config\firefox\autoconfig\local-settings.js'
$mozillaCfgSource = Join-Path $repoRoot 'config\firefox\autoconfig\mozilla.cfg'
$manifestPath = Join-Path $browserRoot 'bundle-manifest.json'
$normalizedEntryExecutableRelativePath = $EntryExecutableRelativePath -replace '\\', '/'
$tempExtractRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("claw-links-windows-bundle-" + [Guid]::NewGuid().ToString('N'))

if ([string]::IsNullOrWhiteSpace($BundleVersion))
{
    throw 'BundleVersion is required.'
}

if ([string]::IsNullOrWhiteSpace($SourceRevision))
{
    throw 'SourceRevision is required.'
}

try
{
    if ($PSCmdlet.ParameterSetName -eq 'Archive')
    {
        $resolvedSourceArchivePath = (Resolve-Path -LiteralPath $SourceArchivePath).Path
        New-Item -ItemType Directory -Force -Path $tempExtractRoot | Out-Null
        Expand-Archive -LiteralPath $resolvedSourceArchivePath -DestinationPath $tempExtractRoot -Force
        $resolvedSourceDirectory = Resolve-ExtractedBundleRoot -ExtractRoot $tempExtractRoot
        $packageArtifactHash = (Get-FileHash -LiteralPath $resolvedSourceArchivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    else
    {
        $resolvedSourceDirectory = (Resolve-Path -LiteralPath $SourceDirectoryPath).Path
    }

    if (Test-Path $bundleRoot)
    {
        Remove-Item -LiteralPath $bundleRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $bundleRoot | Out-Null
    Copy-DirectoryContents -SourceDirectory $resolvedSourceDirectory -DestinationDirectory $bundleRoot

    $sourceExecutablePath = Join-Path $resolvedSourceDirectory $EntryExecutableRelativePath
    $installedExecutablePath = Join-Path $bundleRoot $EntryExecutableRelativePath

    if (!(Test-Path -LiteralPath $installedExecutablePath))
    {
        throw "Expected entry executable at '$installedExecutablePath'. Adjust EntryExecutableRelativePath if the browser executable lives elsewhere in the bundle."
    }

    New-Item -ItemType Directory -Force -Path (Join-Path $bundleRoot 'distribution') | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $bundleRoot 'defaults\pref') | Out-Null

    Copy-Item -LiteralPath $policySource -Destination (Join-Path $bundleRoot 'distribution\policies.json') -Force
    Copy-Item -LiteralPath $localSettingsSource -Destination (Join-Path $bundleRoot 'defaults\pref\local-settings.js') -Force
    Copy-Item -LiteralPath $mozillaCfgSource -Destination (Join-Path $bundleRoot 'mozilla.cfg') -Force

    if ($PSCmdlet.ParameterSetName -eq 'Directory')
    {
        if (!(Test-Path -LiteralPath $sourceExecutablePath))
        {
            throw "Expected source entry executable at '$sourceExecutablePath'."
        }

        $packageArtifactHash = (Get-FileHash -LiteralPath $sourceExecutablePath -Algorithm SHA256).Hash.ToLowerInvariant()
    }

    $installedArtifactHash = (Get-FileHash -LiteralPath $installedExecutablePath -Algorithm SHA256).Hash.ToLowerInvariant()

    $manifest = [ordered]@{
        schemaVersion = 1
        bundleId = "claw-browser-$Platform"
        bundleVersion = $BundleVersion
        platform = $Platform
        architecture = $Architecture
        upstreamProductLine = $UpstreamProductLine
        sourceRevision = $SourceRevision
        releaseChannel = $ReleaseChannel
        packageDigest = "sha256:$packageArtifactHash"
        installedDigest = "sha256:$installedArtifactHash"
        displayName = $DisplayName
        entryExecutableRelativePath = "bundles/$BundleVersion/$normalizedEntryExecutableRelativePath"
        policyRelativePath = "bundles/$BundleVersion/distribution/policies.json"
        supportFiles = @(
            [ordered]@{
                relativePath = "bundles/$BundleVersion/defaults/pref/local-settings.js"
                role = 'autoconfig-loader'
            },
            [ordered]@{
                relativePath = "bundles/$BundleVersion/mozilla.cfg"
                role = 'autoconfig-config'
            }
        )
    }

    New-Item -ItemType Directory -Force -Path $browserRoot | Out-Null
    $manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath -Encoding utf8

    Write-Host "Browser bundle installed to $bundleRoot"
    Write-Host "Bundle manifest written to $manifestPath"
    Write-Host "Entry executable: $installedExecutablePath"
    Write-Host "Package digest: sha256:$packageArtifactHash"
    Write-Host "Installed digest: sha256:$installedArtifactHash"

    if ($PSCmdlet.ParameterSetName -eq 'Directory')
    {
        Write-Host "Note: because SourceDirectoryPath was used, packageDigest currently tracks the source entry executable."
    }
}
finally
{
    if (Test-Path $tempExtractRoot)
    {
        Remove-Item -LiteralPath $tempExtractRoot -Recurse -Force
    }
}
