# Claw Links

`Claw Links` is a cross-platform launcher for opening Discord links in a managed, hardened Firefox lane instead of the user's normal browser session.

The current repo scaffold focuses on:

- architecture for Windows, macOS, and Linux
- self-building Firefox from upstream Mozilla source
- licensing, trademark, and redistribution constraints
- a baseline Firefox enterprise policy set for the managed browser install

## Direction

We are standardizing on:

- browser engine/runtime: Mozilla Firefox
- build source: upstream Mozilla source, built by us
- distribution model: modified, self-built, unbranded browser bundle plus a small launcher
- safety model: dedicated managed Firefox install + dedicated managed profile + optional OS-specific stronger isolation later

Why Firefox:

- open source
- official cross-platform support
- separate profiles are first-class
- enterprise policies work on Windows, macOS, and Linux

## Repo layout

- [docs/architecture.md](/Users/timot/Documents/projects/discord-link/docs/architecture.md)
- [docs/browser-bundle-layout.md](/Users/timot/Documents/projects/discord-link/docs/browser-bundle-layout.md)
- [docs/firefox-build.md](/Users/timot/Documents/projects/discord-link/docs/firefox-build.md)
- [docs/licensing-and-redistribution.md](/Users/timot/Documents/projects/discord-link/docs/licensing-and-redistribution.md)
- [docs/windows-bundle-artifact.md](/Users/timot/Documents/projects/discord-link/docs/windows-bundle-artifact.md)
- [config/firefox/policies.json](/Users/timot/Documents/projects/discord-link/config/firefox/policies.json)
- `src/ClawLinks.Launcher/` - .NET launcher CLI scaffold
- `tests/ClawLinks.Launcher.Tests/` - launcher unit tests

## Current product shape

V1 is intentionally small:

1. Ship a self-built Firefox-derived browser bundle under our own name.
2. Install a locked-down `policies.json` next to that browser.
3. Create a dedicated app-owned profile directory on first run.
4. Launch only untrusted links through that browser/profile pair.

That gets us:

- no borrowed cookies from the user's everyday browser
- no sync or saved-password prompts in the untrusted lane
- forced HTTPS-only mode
- locked-down download behavior
- blocked camera, mic, location, notifications, and screen share requests
- an allowlist-only extension model

## Important note on naming

Because a modified Firefox build cannot be redistributed under Mozilla trademarks without prior written permission, this project should assume a non-Firefox product name and non-Firefox branding from day one. Details are in [docs/licensing-and-redistribution.md](/Users/timot/Documents/projects/discord-link/docs/licensing-and-redistribution.md).

## Next implementation steps

- add the launcher CLI and per-OS installers
- automate upstream Firefox builds for Windows, macOS, and Linux
- decide whether app updates come from our own signed release channel or from user-managed reinstall/upgrade flows
- add optional stronger isolation wrappers:
  - Windows: Windows Sandbox
  - Linux: AppArmor or a sandbox wrapper
  - macOS: sandboxed helper app later

## Launcher scaffold

The launcher is now scaffolded in `.NET` because it gives us a cross-platform CLI while matching the toolchain already available in this workspace.

Current command:

```powershell
dotnet run --project .\src\ClawLinks.Launcher\ClawLinks.Launcher.csproj -- open https://discord.com
```

Current responsibilities:

- validate that the URL is `http` or `https`
- resolve the per-OS app, browser, and profile paths
- discover the managed browser bundle from `browser/bundle-manifest.json`
- install `policies.json` into the manifest-declared bundle layout
- validate that manifest-declared support files exist before launch
- bootstrap a deterministic managed profile with first-run markers and a profile state manifest
- launch the managed browser bundle if one exists at an expected path
- print bundle diagnostics after a successful `open`
- append local diagnostics records under the app data `logs/` directory for both success and failure cases

The bundle manifest now also carries build provenance:

- architecture
- upstream product line
- source revision
- release channel
- package digest
- installed digest

Shared interfaces:

- `IPathResolver`
- `IBrowserBundleBootstrapper`
- `IProfileBootstrapper`
- `IBrowserLauncher`

Windows packaging stub:

```powershell
.\packaging\windows\Install-WindowsBrowserStub.ps1
```

That creates a versioned browser bundle, writes the bundle manifest, and lays down the autoconfig support files alongside `distribution\policies.json`.

Windows smoke test:

```powershell
.\packaging\windows\Invoke-WindowsSmokeTest.ps1
```

That runs the Windows stub install into an isolated app-data root, sets `CLAW_LINKS_APPDATA_ROOT` for the launcher, and verifies that the launcher bootstraps the profile and starts the stub browser process.

Windows real bundle install:

```powershell
.\packaging\windows\Install-WindowsBrowserBundle.ps1 `
  -SourceArchivePath C:\path\to\claw-browser-win64.zip `
  -BundleVersion 128.0.1-build1 `
  -SourceRevision firefox-128.0.1esr
```

For local packaging work, the same script can also install from an extracted bundle directory with `-SourceDirectoryPath`. Using an archive is preferred because `packageDigest` can then track the actual packaged artifact rather than falling back to the source executable hash.
