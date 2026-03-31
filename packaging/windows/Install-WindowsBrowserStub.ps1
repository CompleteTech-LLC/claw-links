param(
    [string]$DestinationRoot = (Join-Path $env:LOCALAPPDATA 'ClawLinks'),
    [string]$BundleVersion = '0.0.0-stub',
    [string]$Platform = 'windows-x64',
    [string]$Architecture = 'x64',
    [string]$UpstreamProductLine = 'stable',
    [string]$SourceRevision = 'stub-local',
    [string]$ReleaseChannel = 'stub'
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
$browserRoot = Join-Path $DestinationRoot 'browser'
$bundleRoot = Join-Path $browserRoot (Join-Path 'bundles' $BundleVersion)
$publishRoot = Join-Path $repoRoot (Join-Path 'artifacts' 'browser-stub-publish')
$stubProject = Join-Path $repoRoot 'tools\ClawLinks.BrowserStub\ClawLinks.BrowserStub.csproj'
$policySource = Join-Path $repoRoot 'config\firefox\policies.json'
$localSettingsSource = Join-Path $repoRoot 'config\firefox\autoconfig\local-settings.js'
$mozillaCfgSource = Join-Path $repoRoot 'config\firefox\autoconfig\mozilla.cfg'
$manifestPath = Join-Path $browserRoot 'bundle-manifest.json'

New-Item -ItemType Directory -Force -Path $bundleRoot | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $bundleRoot 'distribution') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $bundleRoot 'defaults\pref') | Out-Null
New-Item -ItemType Directory -Force -Path $publishRoot | Out-Null

dotnet publish $stubProject -c Release -r win-x64 --self-contained false -o $publishRoot | Out-Host

Copy-Item (Join-Path $publishRoot 'claw-browser.exe') (Join-Path $bundleRoot 'claw-browser.exe') -Force
Copy-Item (Join-Path $publishRoot 'claw-browser.dll') (Join-Path $bundleRoot 'claw-browser.dll') -Force
Copy-Item (Join-Path $publishRoot 'claw-browser.deps.json') (Join-Path $bundleRoot 'claw-browser.deps.json') -Force
Copy-Item (Join-Path $publishRoot 'claw-browser.runtimeconfig.json') (Join-Path $bundleRoot 'claw-browser.runtimeconfig.json') -Force
Copy-Item $policySource (Join-Path $bundleRoot 'distribution\policies.json') -Force
Copy-Item $localSettingsSource (Join-Path $bundleRoot 'defaults\pref\local-settings.js') -Force
Copy-Item $mozillaCfgSource (Join-Path $bundleRoot 'mozilla.cfg') -Force

$artifactPath = Join-Path $bundleRoot 'claw-browser.exe'
$packageArtifactPath = Join-Path $publishRoot 'claw-browser.exe'
$packageArtifactHash = (Get-FileHash -Path $packageArtifactPath -Algorithm SHA256).Hash.ToLowerInvariant()
$installedArtifactHash = (Get-FileHash -Path $artifactPath -Algorithm SHA256).Hash.ToLowerInvariant()

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
    displayName = 'Claw Browser Stub'
    entryExecutableRelativePath = "bundles/$BundleVersion/claw-browser.exe"
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

$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath -Encoding utf8

Write-Host "Stub browser bundle installed to $bundleRoot"
Write-Host "Bundle manifest written to $manifestPath"
