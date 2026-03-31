# Windows Bundle Artifact Contract

This document defines the first real Windows browser artifact contract for `Claw Links`.

The goal is to make the first Firefox-derived Windows runtime installable through `Install-WindowsBrowserBundle.ps1` without inventing a custom installer format too early.

## Contract Summary

The first real Windows browser artifact should be:

- a `.zip` archive
- produced from a self-built Firefox-derived Windows package
- portable and unpackable without a system-wide installer
- installed into the managed app-owned browser root by `Install-WindowsBrowserBundle.ps1`

Recommended artifact filename pattern:

- `claw-browser-windows-x64-<bundleVersion>.zip`

Examples:

- `claw-browser-windows-x64-128.0.1-build1.zip`
- `claw-browser-windows-arm64-128.0.1-build1.zip`

## Why Zip First

Zip is the simplest artifact shape for the first Windows release path because it gives us:

- a single packaged artifact to hash for `packageDigest`
- a portable layout that can be tested without writing registry keys
- a clean handoff between build automation and the launcher installer path

We can add an `.msi` or bootstrapper later, but the browser runtime itself should first exist as a portable archive.

## Required Archive Shape

The archive must expand to a runnable portable browser directory.

The installer currently supports either:

- files directly at the archive root
- a single top-level directory that contains the browser files

For the first contract, prefer this extracted layout:

```text
<archive root>/
  firefox.exe
  firefox.dll
  omni.ja
  platform.ini
  application.ini
  dependentlibs.list
  [other runtime files produced by the browser build]
```

Important rule:

- treat the browser payload as a complete runnable application directory
- do not cherry-pick only a few files from a build output

If the build output uses a rebranded executable later, pass that path to the installer with `-EntryExecutableRelativePath`. For the first contract, prefer `firefox.exe` because it matches the natural Windows output of Firefox-derived packaging.

## Required Installer Inputs

The Windows installer script requires these values when installing a real bundle:

- `BundleVersion`
- `SourceRevision`
- `UpstreamProductLine`
- `ReleaseChannel`
- `Architecture`
- `DisplayName`
- `EntryExecutableRelativePath`

Minimum viable install command:

```powershell
.\packaging\windows\Install-WindowsBrowserBundle.ps1 `
  -SourceArchivePath C:\path\to\claw-browser-windows-x64-128.0.1-build1.zip `
  -BundleVersion 128.0.1-build1 `
  -SourceRevision firefox-128.0.1esr `
  -UpstreamProductLine esr `
  -ReleaseChannel staging
```

## Installed Layout

After installation, the managed browser root should look like:

```text
%LOCALAPPDATA%\ClawLinks\browser\
  bundle-manifest.json
  bundles\
    <bundleVersion>\
      firefox.exe
      firefox.dll
      omni.ja
      platform.ini
      application.ini
      dependentlibs.list
      distribution\
        policies.json
      defaults\
        pref\
          local-settings.js
      mozilla.cfg
      [other runtime files from the packaged browser]
```

Notes:

- `distribution\policies.json`, `defaults\pref\local-settings.js`, and `mozilla.cfg` are owned by `Claw Links` packaging and are overlaid during install
- the runtime files come from the packaged browser archive
- the launcher reads `browser\bundle-manifest.json` and then launches the manifest-declared executable

## Manifest Expectations

The first real Windows artifact should produce a manifest shaped like this after installation:

```json
{
  "schemaVersion": 1,
  "bundleId": "claw-browser-windows-x64",
  "bundleVersion": "128.0.1-build1",
  "platform": "windows-x64",
  "architecture": "x64",
  "upstreamProductLine": "esr",
  "sourceRevision": "firefox-128.0.1esr",
  "releaseChannel": "staging",
  "packageDigest": "sha256:<zip hash>",
  "installedDigest": "sha256:<installed firefox.exe hash>",
  "displayName": "Claw Browser",
  "entryExecutableRelativePath": "bundles/128.0.1-build1/firefox.exe",
  "policyRelativePath": "bundles/128.0.1-build1/distribution/policies.json",
  "supportFiles": [
    {
      "relativePath": "bundles/128.0.1-build1/defaults/pref/local-settings.js",
      "role": "autoconfig-loader"
    },
    {
      "relativePath": "bundles/128.0.1-build1/mozilla.cfg",
      "role": "autoconfig-config"
    }
  ]
}
```

Digest policy for the first contract:

- `packageDigest` should be the SHA-256 of the zip archive
- `installedDigest` should be the SHA-256 of the installed entry executable

That keeps the initial implementation simple while still letting us pin both the shipped artifact and the installed launcher target.

## Artifact Production Rules

The first real Windows artifact should:

- come from a pinned upstream Firefox revision
- use project-owned branding in the surrounding product metadata and docs
- avoid bundling user profile state
- avoid assuming registry installation
- remain runnable from an extracted directory

The first real Windows artifact should not:

- be an `.exe` installer as the only distributable browser payload
- depend on the user already having Firefox installed
- mix browser payload files with launcher logs or profile data

## Validation Checklist

Before we call a Windows bundle artifact acceptable, confirm:

- the zip extracts cleanly
- the extracted directory contains the expected entry executable
- the installer can write `bundle-manifest.json`
- the launcher can bootstrap the managed profile
- the launcher can start the installed executable
- the managed install contains the policy and autoconfig files in the expected locations

## Current Gap

This contract defines how the first real Windows bundle should look and how it should be installed.

It does not yet define:

- the actual Firefox build automation that produces the archive
- final executable renaming or branding inside the browser binary
- an outer Windows installer or updater

Those are follow-on steps once the first real portable archive exists.
