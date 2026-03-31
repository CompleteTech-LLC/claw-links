# Firefox Build Notes

This project intends to compile its browser runtime from upstream Mozilla source.

## Upstream build references

- Linux: <https://firefox-source-docs.mozilla.org/setup/linux_build.html>
- macOS: <https://firefox-source-docs.mozilla.org/setup/macos_build.html>
- Windows: <https://firefox-source-docs.mozilla.org/setup/windows_build.html>

## Common build flow

Mozilla's documented bootstrap flow is broadly the same across platforms:

1. install host prerequisites
2. fetch Mozilla's bootstrap helper
3. run bootstrap
4. build with `./mach build`
5. package with `./mach package` when distributing

Mozilla also documents `Artifact Mode`, but this project should prefer full builds for release artifacts because we want a self-built distributable browser, not a developer convenience build.

## Linux

Mozilla's Linux guide says a Debian-based host such as Ubuntu can start with:

```bash
sudo apt update && sudo apt install curl python3 python3-venv git
curl -LO https://raw.githubusercontent.com/mozilla-firefox/firefox/refs/heads/main/python/mozboot/bin/bootstrap.py
python3 bootstrap.py
cd firefox
git pull
./mach build
```

Reference:

- <https://firefox-source-docs.mozilla.org/setup/linux_build.html>

## macOS

Mozilla's macOS guide says to install Homebrew and Xcode first, then:

```bash
curl -L https://raw.githubusercontent.com/mozilla-firefox/firefox/refs/heads/main/python/mozboot/bin/bootstrap.py -O
python3 bootstrap.py
cd firefox
git pull
./mach build
./mach package
```

Important distribution note from Mozilla:

- local testing does not require code signing
- distributable packaging on Apple Silicon needs signing
- downloaded apps also need notarization for smooth Finder launch behavior on modern macOS

References:

- <https://firefox-source-docs.mozilla.org/setup/macos_build.html>
- <https://firefox-source-docs.mozilla.org/contributing/signing/signing_macos_build.html>

## Windows

Mozilla's Windows guide uses MozillaBuild and the MozillaBuild shell:

```bash
cd c:/
mkdir mozilla-source
cd mozilla-source
wget https://raw.githubusercontent.com/mozilla-firefox/firefox/refs/heads/main/python/mozboot/bin/bootstrap.py
python3 bootstrap.py
cd c:/mozilla-source/firefox
git pull origin main
./mach build
```

Important Windows notes from Mozilla:

- use `C:\\mozilla-build\\start-shell.bat`
- keep source and MozillaBuild paths free of spaces
- antivirus exclusions matter a lot for build reliability and speed

Reference:

- <https://firefox-source-docs.mozilla.org/setup/windows_build.html>

## Packaging expectations

For our purposes, the release pipeline should produce:

- Windows: portable app directory first, installer later
- macOS: `.dmg`
- Linux: tarball first, distro packaging later

The launcher project should treat the browser runtime as a versioned embedded dependency, not a system browser assumption.

For Windows specifically, the first real browser artifact contract is a portable `.zip` archive that expands to a runnable browser directory and is then installed by [packaging/windows/Install-WindowsBrowserBundle.ps1](/Users/timot/Documents/projects/discord-link/packaging/windows/Install-WindowsBrowserBundle.ps1). The expected artifact shape is documented in [docs/windows-bundle-artifact.md](/Users/timot/Documents/projects/discord-link/docs/windows-bundle-artifact.md).

## Build policy for this project

- build from a pinned upstream Firefox release tag or commit
- track Mozilla stable or ESR releases closely
- publish source availability for the MPL-covered portions we redistribute
- never ship a modified browser under Firefox branding without Mozilla's written permission

## Recommended release discipline

- maintain one tracked upstream line at a time
- document the exact upstream revision used for each release
- automate rebuilds quickly after Mozilla security releases
- keep a changelog that distinguishes upstream Mozilla changes from our project changes
