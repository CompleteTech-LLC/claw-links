# Architecture

## Goals

- Make random links from Discord safer to open without forcing a full VM workflow every time.
- Work on Windows, macOS, and Linux from the start.
- Use an open-source browser that we can build ourselves.
- Keep the untrusted-link lane separate from the user's normal browser identity.
- Keep the initial product small enough to ship and maintain.

## Non-goals for V1

- Intercept every click directly inside Discord.
- Fully virtualize or containerize every link open on every platform.
- Anonymity against destination sites.
- Perfect malware containment after a user intentionally downloads and runs software.

## Threat model

We are primarily reducing:

- OAuth phishing that depends on the victim already being logged in
- credential theft from cookie/session reuse
- accidental downloads from sketchy pages
- permission abuse such as notification, microphone, or screen-share prompts
- extension abuse in the unsafe browsing lane

We are not fully eliminating:

- social engineering after a user logs into an account inside the unsafe browser
- kernel or browser zero-days
- malware executed outside the browser
- destination-site tracking of the unsafe browser itself

## Browser choice

Firefox is the best fit because Mozilla documents all the pieces we need:

- separate profiles
- enterprise policies across Windows, macOS, and Linux
- supported build instructions for all three operating systems

Official references:

- Firefox profiles: <https://support.mozilla.org/en-US/kb/profile-management>
- Firefox policy templates: <https://mozilla.github.io/policy-templates/>
- Linux build: <https://firefox-source-docs.mozilla.org/setup/linux_build.html>
- macOS build: <https://firefox-source-docs.mozilla.org/setup/macos_build.html>
- Windows build: <https://firefox-source-docs.mozilla.org/setup/windows_build.html>

## Product shape

The shipped app has two moving parts:

1. `claw-links` launcher
2. self-built Firefox-derived browser bundle

The launcher receives a URL and opens it in a dedicated managed browser lane.

The browser lane is composed of:

- a dedicated browser install owned by the app
- a dedicated app-owned profile directory
- a hardened `policies.json`

## Why a managed profile instead of the user's own Firefox

A copied or borrowed user profile is too fragile and too easy to weaken over time.

For V1, the app should create a fresh deterministic profile on first run and keep it separate forever. That satisfies the requirement that the profile be predefined and hardened, while avoiding cross-machine profile drift.

Recommended profile location pattern:

- Windows: `%LOCALAPPDATA%\\ClawLinks\\profile`
- macOS: `~/Library/Application Support/ClawLinks/profile`
- Linux: `${XDG_DATA_HOME:-~/.local/share}/claw-links/profile`

Recommended browser install location pattern:

- Windows: `%LOCALAPPDATA%\\ClawLinks\\browser`
- macOS: `~/Library/Application Support/ClawLinks/browser`
- Linux: `${XDG_DATA_HOME:-~/.local/share}/claw-links/browser`

## Baseline hardening

Mozilla's policy system lets us put `policies.json` next to the installed browser:

- Windows: `distribution\\policies.json` next to the executable
- macOS: `Firefox.app/Contents/Resources/distribution/policies.json`
- Linux: `firefox/distribution/policies.json` or `/etc/firefox/policies`

Official location reference:

- <https://mozilla.github.io/policy-templates/>

The baseline policy set for this project should include:

- `DisableFirefoxAccounts: true`
- `DisableTelemetry: true`
- `DisableFirefoxStudies: true`
- `OfferToSaveLogins: false`
- `PasswordManagerEnabled: false`
- `HttpsOnlyMode: "force_enabled"`
- `PromptForDownloadLocation: true`
- `SanitizeOnShutdown` with `Cache`, `Cookies`, `FormData`, `History`, `Sessions`, and `SiteSettings` enabled and locked
- `Permissions` blocking new requests for camera, microphone, location, notifications, and screen share
- `EnableTrackingProtection` set to strict and locked
- `ExtensionSettings` with `*` blocked by default

The checked-in baseline lives at [config/firefox/policies.json](/Users/timot/Documents/projects/discord-link/config/firefox/policies.json).

## Extension strategy

Default stance:

- no arbitrary user-installed extensions in the unsafe browser lane

This is important because a "safe links" browser that allows random add-ons becomes another attack surface.

If we later decide to ship an extension such as uBlock Origin, it should be force-installed through `ExtensionSettings` rather than left open-ended.

## Launch model

The launcher should:

1. validate that the input is an `http` or `https` URL
2. normalize and log the request locally for troubleshooting
3. ensure the managed profile exists
4. ensure the managed browser build exists
5. open the URL in the managed browser install with the managed profile

The browser invocation should be direct and deterministic rather than "open in default browser."

## Cross-platform isolation strategy

The cross-platform baseline is the managed Firefox lane. Stronger isolation is additive and OS-specific.

Windows:

- V1 baseline: managed Firefox lane
- V2 optional: open through Windows Sandbox for high-risk links

macOS:

- V1 baseline: managed Firefox lane
- V2 optional: small sandboxed helper app that launches the browser with stricter surrounding controls

Linux:

- V1 baseline: managed Firefox lane
- V2 optional: AppArmor profile or sandbox wrapper

This keeps V1 shippable while still leaving a clear path to stronger isolation later.

## Build and release strategy

We should build Firefox ourselves from upstream Mozilla source and package our own browser bundle for each operating system.

That implies:

- our own CI for Windows, macOS, and Linux builds
- our own release cadence when Mozilla ships security updates
- our own product name, icons, installer naming, and update story

## Update strategy

We have two realistic options:

1. disable in-browser updates and ship full browser updates through our app/release channel
2. host our own Firefox-derived update service and point `AppUpdateURL` at it

V1 should use option 1 because it is much simpler operationally.

## Operational risks

- We become responsible for keeping pace with Mozilla security releases.
- A stale self-built browser would be worse than using upstream Firefox directly.
- macOS packaging is more operationally complex because distributable builds need signing, and notarization matters for smooth distribution.
- Modified builds cannot be redistributed as Firefox without Mozilla's written permission.

## Naming recommendation

Use `Claw Links` as the working product name. Do not use Firefox marks in the product name, app icon, or installer branding unless Mozilla grants permission.
