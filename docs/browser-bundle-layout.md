# Browser Bundle Layout

`Claw Links` now expects packaging to declare the managed browser layout explicitly with `browser/bundle-manifest.json`.

## Why the manifest exists

- the launcher should not guess which browser executable is current
- packaging needs a stable contract for versioned bundles under `browser/`
- future support files such as autoconfig should be declared alongside the executable

## Manifest location

- Windows: `%LOCALAPPDATA%\ClawLinks\browser\bundle-manifest.json`
- macOS: `~/Library/Application Support/ClawLinks/browser/bundle-manifest.json`
- Linux: `${XDG_DATA_HOME:-~/.local/share}/claw-links/browser/bundle-manifest.json`

## Manifest shape

Schema version `1` uses:

- `bundleId`
- `bundleVersion`
- `platform`
- `architecture`
- `upstreamProductLine`
- `sourceRevision`
- `releaseChannel`
- `packageDigest`
- `installedDigest`
- `displayName`
- `entryExecutableRelativePath`
- `policyRelativePath`
- `supportFiles[]`

All manifest paths must be relative to the browser root.

The new provenance fields are intended to pin the exact Firefox-derived build the launcher is opening:

- `architecture`: CPU target such as `x64` or `arm64`
- `upstreamProductLine`: the upstream Firefox line such as `stable`, `esr`, `beta`, or `nightly`
- `sourceRevision`: upstream tag, commit, or internal revision identifier used to build the browser
- `releaseChannel`: our distribution lane such as `production`, `staging`, `qa`, or another internal release track
- `packageDigest`: digest of the package or archive selected for installation, formatted as `sha256:<hex>`
- `installedDigest`: digest of the installed browser payload, formatted as `sha256:<hex>`

`upstreamProductLine` is intentionally separate from `releaseChannel`.

Examples:

- upstream product line `esr` + release channel `production`
- upstream product line `stable` + release channel `staging`

## Windows stub layout

The Windows packaging stub writes:

- `browser/bundle-manifest.json`
- `browser/bundles/<version>/claw-browser.exe`
- `browser/bundles/<version>/distribution/policies.json`
- `browser/bundles/<version>/defaults/pref/local-settings.js`
- `browser/bundles/<version>/mozilla.cfg`

For the current stub:

- `packageDigest` is computed from the published stub `claw-browser.exe`
- `installedDigest` is computed from the installed `claw-browser.exe`

That means the two values will currently match. In the real pipeline, `packageDigest` should come from the packaged browser archive while `installedDigest` can evolve to a fuller installed-bundle digest later.

The launcher now treats manifest-declared support files as required. If a listed support file is missing, launch fails with a clear validation error before the browser is started.

## First Real Windows Artifact

The first non-stub Windows browser artifact should be a portable `.zip` containing a runnable Firefox-derived browser directory, not a Windows installer-only payload.

The expected archive shape and installed layout are defined in [docs/windows-bundle-artifact.md](/Users/timot/Documents/projects/discord-link/docs/windows-bundle-artifact.md).

That contract standardizes:

- the first archive format
- the expected extracted folder shape
- the managed installed layout under `browser/bundles/<version>/`
- the first real Windows manifest example
- the digest policy for `packageDigest` and `installedDigest`

## Local diagnostics log

Each `open` attempt appends a local diagnostics record:

- Windows: `%LOCALAPPDATA%\ClawLinks\logs\open.ndjson`
- macOS: `~/Library/Application Support/ClawLinks/logs/open.ndjson`
- Linux: `${XDG_DATA_HOME:-~/.local/share}/claw-links/logs/open.ndjson`

The log records both successful launches and failures, including manifest provenance and digest data when a bundle was successfully prepared.

## Autoconfig decision

We should use browser-level autoconfig in addition to `policies.json`.

Rationale:

- `policies.json` remains the primary enterprise control surface
- autoconfig is the right place for locked prefs that Mozilla policies do not cover cleanly
- shipping `mozilla.cfg` and `defaults/pref/local-settings.js` keeps those locks bundle-owned instead of profile-owned

For the current stub, autoconfig owns these locked prefs:

- `browser.aboutwelcome.enabled = false`
- `browser.newtabpage.enabled = false`
- `browser.shell.checkDefaultBrowser = false`
- `browser.startup.homepage = about:blank`
- `browser.startup.page = 0`
