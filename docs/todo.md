# Todo

This checklist consolidates the current roadmap for `Claw Links` into one working plan.

Status key:

- `[x]` done enough for the current scaffold
- `[ ]` planned or not started
- `P0` must happen before a credible V1
- `P1` important follow-up after the V1 path is stable
- `P2` later or optional

## P0 Foundation

- [x] Define the cross-platform product direction and threat model.
- [x] Document browser bundle layout expectations per OS.
- [x] Document licensing, redistribution, and trademark constraints.
- [x] Check in the baseline Firefox enterprise policy set.
- [x] Scaffold the launcher CLI with tests.
- [x] Bootstrap a deterministic managed profile on first run.
- [x] Add a Windows browser bundle stub installer that writes `browser/bundle-manifest.json`.
- [ ] Add a root `.sln` so repo-level `dotnet build` and `dotnet test` work cleanly.
- [ ] Decide the release lane to track first: Firefox `stable` or `esr`.

## P0 Launcher V1

- [x] Validate that only `http` and `https` URLs are accepted.
- [x] Resolve app, browser, profile, and logs paths per operating system.
- [x] Load `browser/bundle-manifest.json` instead of guessing bundle paths.
- [x] Validate required support files before launch.
- [x] Install `policies.json` into the manifest-declared bundle layout.
- [x] Write local diagnostics records for successful and failed open attempts.
- [ ] Add CLI options for diagnostics and troubleshooting.
- [ ] Add a machine-readable output mode for automation and installer integration.
- [ ] Decide how the launcher should behave when no managed bundle is installed:
  - fail with remediation guidance
  - optionally invoke an installer/bootstrap flow later
- [ ] Add end-to-end tests for the full `open` flow on all supported operating systems.

## P0 Browser Bundle Layout

- [x] Define the manifest schema with provenance and digest fields.
- [x] Require manifest-declared support files to exist before launch.
- [x] Document the Windows stub layout.
- [x] Define the first real Windows bundle artifact contract and expected folder layout.
- [ ] Define the macOS bundle layout contract in the manifest and docs.
- [ ] Define the Linux bundle layout contract in the manifest and docs.
- [ ] Standardize naming for the shipped browser executable and display name across OSes.
- [ ] Decide whether bundle digests cover only the main executable or the full installed payload.

## P0 Packaging And Release

- [x] Add a Windows stub packaging script for local validation.
- [x] Add a Windows installer path for a prebuilt browser bundle archive or directory.
- [ ] Replace the Windows stub path with a real Firefox-derived packaging flow.
- [ ] Add macOS packaging automation that outputs an app bundle and distributable artifact.
- [ ] Add Linux packaging automation that outputs a tarball first.
- [ ] Pin the exact upstream revision used for each browser build.
- [ ] Capture `architecture`, `upstreamProductLine`, `sourceRevision`, and `releaseChannel` in every produced manifest.
- [ ] Compute `packageDigest` from the packaged browser archive, not the installed stub output.
- [ ] Publish MPL notices, source availability details, and release metadata with every browser release.
- [ ] Decide installer strategy per OS:
  - Windows installer or bootstrapper
  - macOS app distribution plus signing/notarization path
  - Linux tarball extraction plus optional distro packaging later
- [ ] Decide the V1 update path:
  - app-managed full browser updates
  - no in-browser updater

## P1 CI And Developer Workflow

- [ ] Add a CI workflow that builds the launcher and runs tests on Windows, macOS, and Linux.
- [ ] Add CI validation for checked-in JSON and packaging manifests.
- [ ] Add CI checks that fail if runtime state or bundled browser payloads are accidentally committed.
- [x] Add a documented local dev flow for installing a stub bundle and running the launcher against it.
- [ ] Add release checklists for security-update turnaround after upstream Mozilla releases.

## P1 Installer And Distribution UX

- [ ] Decide where the managed browser bundle is installed per OS in installer flows versus portable/dev flows.
- [ ] Add a first-run bootstrap path that can create app directories safely and idempotently.
- [ ] Decide whether the launcher should install file/protocol handlers or remain a manual CLI first.
- [ ] Add user-facing error messages for:
  - missing bundle
  - stale bundle
  - invalid manifest
  - missing support files
  - launch failures
- [ ] Document signer/notarization requirements before macOS distribution work begins.

## P1 Hardening Follow-Up

- [x] Separate the managed profile from the user's normal browser state.
- [x] Lock down baseline browser policies and autoconfig-owned prefs.
- [ ] Review whether additional locked prefs belong in autoconfig instead of the profile.
- [ ] Decide whether to ship a force-installed extension such as an ad/tracker blocker.
- [ ] Review download handling and decide whether more restrictive defaults are needed for V1.
- [ ] Add a documented policy for clearing or retaining diagnostics logs.

## P2 Optional Stronger Isolation

- [ ] Prototype a Windows Sandbox wrapper for high-risk links.
- [ ] Prototype a Linux sandbox wrapper or AppArmor profile.
- [ ] Explore a macOS sandboxed helper app for a later release.
- [ ] Define how the launcher decides when to use baseline mode versus stronger isolation.

## Open Decisions

- [ ] Choose the first tracked upstream line: `stable` or `esr`.
- [ ] Choose the V1 update model.
- [ ] Choose whether missing bundles should be installer-driven or launcher-fatal.
- [ ] Choose the initial release artifact shape for each OS.
- [ ] Choose whether a force-installed extension is part of V1.

## Suggested Next Step

- [ ] Implement macOS and Linux browser bundle layout docs and packaging stubs so the existing Windows-only bundle contract becomes truly cross-platform.
