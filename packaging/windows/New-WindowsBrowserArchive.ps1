param(
    [Parameter(Mandatory = $true)]
    [string]$SourceDirectoryPath,
    [Parameter(Mandatory = $true)]
    [string]$BundleVersion,
    [Parameter(Mandatory = $true)]
    [string]$SourceRevision,
    [string]$Platform = 'windows-x64',
    [string]$Architecture = 'x64',
    [string]$UpstreamProductLine = 'esr',
    [string]$ReleaseChannel = 'staging',
    [string]$DisplayName = 'Claw Browser',
    [string]$EntryExecutableRelativePath = 'firefox.exe',
    [ValidateSet('warning', 'strict', 'off')]
    [string]$RuntimeValidationMode = 'warning',
    [string]$OutputDirectory = ''
)

$ErrorActionPreference = 'Stop'

function Test-EsrBundleVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    return $Value -match '^\d+\.\d+\.\d+esr-build\d+$'
}

function Get-RelativePathNormalized {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    return $Value -replace '\\', '/'
}

function Get-RuntimeValidationReport {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredMarkerFiles,
        [Parameter(Mandatory = $true)]
        [string[]]$ForbiddenRelativePaths
    )

    $requiredMarkerStatus = [ordered]@{}
    foreach ($marker in $RequiredMarkerFiles)
    {
        $requiredMarkerStatus[$marker] = Test-Path -LiteralPath (Join-Path $SourceDirectory $marker)
    }

    $forbiddenPathStatus = [ordered]@{}
    foreach ($relativePath in $ForbiddenRelativePaths)
    {
        $forbiddenPathStatus[$relativePath] = Test-Path -LiteralPath (Join-Path $SourceDirectory $relativePath)
    }

    $missingMarkers = @($requiredMarkerStatus.GetEnumerator() |
        Where-Object { -not $_.Value } |
        Select-Object -ExpandProperty Key)

    $presentForbiddenPaths = @($forbiddenPathStatus.GetEnumerator() |
        Where-Object { $_.Value } |
        Select-Object -ExpandProperty Key)

    return [ordered]@{
        requiredMarkerStatus = $requiredMarkerStatus
        forbiddenPathStatus = $forbiddenPathStatus
        missingMarkers = $missingMarkers
        presentForbiddenPaths = $presentForbiddenPaths
        hasIssues = (($missingMarkers.Count + $presentForbiddenPaths.Count) -gt 0)
    }
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
$resolvedSourceDirectory = (Resolve-Path -LiteralPath $SourceDirectoryPath).Path
$normalizedEntryExecutableRelativePath = Get-RelativePathNormalized -Value $EntryExecutableRelativePath
$entryExecutablePath = Join-Path $resolvedSourceDirectory $EntryExecutableRelativePath
$artifactBaseName = "claw-browser-$Platform-$BundleVersion"
$tempArchiveRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("claw-links-windows-archive-" + [Guid]::NewGuid().ToString('N'))
$stagingRoot = Join-Path $tempArchiveRoot $artifactBaseName
$recommendedRuntimeMarkers = @(
    'application.ini',
    'platform.ini',
    'dependentlibs.list',
    'omni.ja'
)
$forbiddenSourcePaths = @(
    'distribution\policies.json',
    'defaults\pref\local-settings.js',
    'mozilla.cfg',
    'browser\bundle-manifest.json',
    'logs\open.ndjson',
    'profile\.claw-links-profile',
    'profile\.claw-links-first-run-complete',
    'profile\claw-links-profile-state.json'
)

if ([string]::IsNullOrWhiteSpace($BundleVersion))
{
    throw 'BundleVersion is required.'
}

if ([string]::IsNullOrWhiteSpace($SourceRevision))
{
    throw 'SourceRevision is required.'
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $repoRoot 'artifacts\release\windows'
}

$artifactPath = Join-Path $OutputDirectory "$artifactBaseName.zip"
$metadataPath = Join-Path $OutputDirectory "$artifactBaseName.metadata.json"

if ($UpstreamProductLine -eq 'esr' -and -not (Test-EsrBundleVersion -Value $BundleVersion))
{
    throw "BundleVersion '$BundleVersion' must match the adopted ESR format '<upstreamVersion>esr-buildN', for example '128.10.0esr-build1'."
}

if (!(Test-Path -LiteralPath $entryExecutablePath))
{
    throw "Expected entry executable at '$entryExecutablePath'."
}

if ($RuntimeValidationMode -ne 'off')
{
    $validationReport = Get-RuntimeValidationReport `
        -SourceDirectory $resolvedSourceDirectory `
        -RequiredMarkerFiles $recommendedRuntimeMarkers `
        -ForbiddenRelativePaths $forbiddenSourcePaths

    if ($validationReport.hasIssues)
    {
        $validationMessages = @()

        if ($validationReport.missingMarkers.Count -gt 0)
        {
            $validationMessages += "Missing recommended Firefox runtime marker file(s): $($validationReport.missingMarkers -join ', ')"
        }

        if ($validationReport.presentForbiddenPaths.Count -gt 0)
        {
            $validationMessages += "Source directory contains app-owned or runtime-state file(s) that should not be packaged: $($validationReport.presentForbiddenPaths -join ', ')"
        }

        $validationMessage = $validationMessages -join ' '

        if ($RuntimeValidationMode -eq 'strict')
        {
            throw "Runtime validation failed. $validationMessage"
        }

        Write-Warning $validationMessage
    }
}
else
{
    $validationReport = [ordered]@{
        requiredMarkerStatus = [ordered]@{}
        forbiddenPathStatus = [ordered]@{}
        missingMarkers = @()
        presentForbiddenPaths = @()
        hasIssues = $false
    }
}

try
{
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

    if (Test-Path -LiteralPath $artifactPath)
    {
        Remove-Item -LiteralPath $artifactPath -Force
    }

    if (Test-Path -LiteralPath $metadataPath)
    {
        Remove-Item -LiteralPath $metadataPath -Force
    }

    New-Item -ItemType Directory -Force -Path $stagingRoot | Out-Null
    Copy-Item -Path (Join-Path $resolvedSourceDirectory '*') -Destination $stagingRoot -Recurse -Force

    Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $artifactPath -CompressionLevel Optimal

    $packageArtifactHash = (Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $metadata = [ordered]@{
        schemaVersion = 1
        artifactName = "$artifactBaseName.zip"
        artifactPath = (Resolve-Path -LiteralPath $artifactPath).Path
        bundleVersion = $BundleVersion
        platform = $Platform
        architecture = $Architecture
        upstreamProductLine = $UpstreamProductLine
        sourceRevision = $SourceRevision
        releaseChannel = $ReleaseChannel
        displayName = $DisplayName
        entryExecutableRelativePath = $normalizedEntryExecutableRelativePath
        packageDigest = "sha256:$packageArtifactHash"
        sourceDirectory = $resolvedSourceDirectory
        createdUtc = [DateTimeOffset]::UtcNow.ToString('O')
        runtimeValidationMode = $RuntimeValidationMode
        runtimeValidation = [ordered]@{
            requiredMarkerStatus = $validationReport.requiredMarkerStatus
            forbiddenPathStatus = $validationReport.forbiddenPathStatus
            missingMarkers = $validationReport.missingMarkers
            presentForbiddenPaths = $validationReport.presentForbiddenPaths
            hasIssues = $validationReport.hasIssues
        }
    }

    $metadata | ConvertTo-Json -Depth 5 | Set-Content -Path $metadataPath -Encoding utf8

    Write-Host "Windows browser archive created."
    Write-Host "Artifact: $artifactPath"
    Write-Host "Metadata: $metadataPath"
    Write-Host "Package digest: sha256:$packageArtifactHash"
    Write-Host "Bundle version: $BundleVersion"
    Write-Host "Source revision: $SourceRevision"
    Write-Host "Runtime validation mode: $RuntimeValidationMode"
}
finally
{
    if (Test-Path -LiteralPath $tempArchiveRoot)
    {
        Remove-Item -LiteralPath $tempArchiveRoot -Recurse -Force
    }
}
