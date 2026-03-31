param(
    [string]$AppDataRoot = (Join-Path (Join-Path (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)) 'artifacts') 'windows-smoke\ClawLinks'),
    [string]$BundleVersion = '0.0.0-smoke',
    [string]$Url = 'https://discord.com'
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
$installScript = Join-Path $scriptRoot 'Install-WindowsBrowserStub.ps1'
$launcherProject = Join-Path $repoRoot 'src\ClawLinks.Launcher\ClawLinks.Launcher.csproj'
$manifestPath = Join-Path $AppDataRoot 'browser\bundle-manifest.json'
$profileStatePath = Join-Path $AppDataRoot 'profile\claw-links-profile-state.json'
$diagnosticsLogPath = Join-Path $AppDataRoot 'logs\open.ndjson'
$markerPath = Join-Path $AppDataRoot 'smoke\browser-stub-launch.txt'

if (Test-Path $AppDataRoot)
{
    Remove-Item -LiteralPath $AppDataRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $AppDataRoot | Out-Null

& $installScript -DestinationRoot $AppDataRoot -BundleVersion $BundleVersion

$previousAppDataRoot = $env:CLAW_LINKS_APPDATA_ROOT
$previousMarkerPath = $env:CLAW_LINKS_SMOKE_MARKER_PATH

try
{
    $env:CLAW_LINKS_APPDATA_ROOT = $AppDataRoot
    $env:CLAW_LINKS_SMOKE_MARKER_PATH = $markerPath

    dotnet run --project $launcherProject -- open $Url | Out-Host

    if ($LASTEXITCODE -ne 0)
    {
        throw "Launcher smoke test failed with exit code $LASTEXITCODE."
    }

    Start-Sleep -Milliseconds 500

    if (!(Test-Path $manifestPath))
    {
        throw "Smoke test expected bundle manifest at '$manifestPath'."
    }

    if (!(Test-Path $profileStatePath))
    {
        throw "Smoke test expected profile state manifest at '$profileStatePath'."
    }

    if (!(Test-Path $markerPath))
    {
        throw "Smoke test expected browser stub launch marker at '$markerPath'."
    }

    Write-Host "Windows smoke test completed."
    Write-Host "App data root: $AppDataRoot"
    Write-Host "Bundle manifest: $manifestPath"
    Write-Host "Profile state: $profileStatePath"
    Write-Host "Diagnostics log: $diagnosticsLogPath"
    Write-Host "Stub launch marker: $markerPath"

    if (Test-Path $diagnosticsLogPath)
    {
        Write-Host
        Write-Host "Diagnostics log contents:"
        Get-Content -Path $diagnosticsLogPath | Out-Host
    }
}
finally
{
    if ($null -eq $previousAppDataRoot)
    {
        Remove-Item Env:CLAW_LINKS_APPDATA_ROOT -ErrorAction SilentlyContinue
    }
    else
    {
        $env:CLAW_LINKS_APPDATA_ROOT = $previousAppDataRoot
    }

    if ($null -eq $previousMarkerPath)
    {
        Remove-Item Env:CLAW_LINKS_SMOKE_MARKER_PATH -ErrorAction SilentlyContinue
    }
    else
    {
        $env:CLAW_LINKS_SMOKE_MARKER_PATH = $previousMarkerPath
    }
}
