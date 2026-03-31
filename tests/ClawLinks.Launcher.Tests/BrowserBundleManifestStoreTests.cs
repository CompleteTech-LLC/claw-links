using ClawLinks.Launcher.Application;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class BrowserBundleManifestStoreTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "claw-links-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsBuildProvenanceFields()
    {
        Directory.CreateDirectory(_tempRoot);

        var manifestPath = Path.Combine(_tempRoot, "bundle-manifest.json");
        var manifest = new BrowserBundleManifest(
            SchemaVersion: 1,
            BundleId: "claw-browser-windows-x64",
            BundleVersion: "128.0.0-claw.1",
            Platform: "windows-x64",
            Architecture: "x64",
            UpstreamProductLine: "esr",
            SourceRevision: "firefox-128.0esr-build1",
            ReleaseChannel: "production",
            PackageDigest: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            InstalledDigest: "sha256:fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210",
            DisplayName: "Claw Browser",
            EntryExecutableRelativePath: "bundles/128.0.0-claw.1/claw-browser.exe",
            PolicyRelativePath: "bundles/128.0.0-claw.1/distribution/policies.json",
            SupportFiles:
            [
                new BrowserSupportFile("bundles/128.0.0-claw.1/defaults/pref/local-settings.js", "autoconfig-loader"),
                new BrowserSupportFile("bundles/128.0.0-claw.1/mozilla.cfg", "autoconfig-config")
            ]);

        await BrowserBundleManifestStore.SaveAsync(manifestPath, manifest, CancellationToken.None);
        var loadedManifest = await BrowserBundleManifestStore.LoadAsync(manifestPath, CancellationToken.None);

        Assert.Equal(manifest.UpstreamProductLine, loadedManifest.UpstreamProductLine);
        Assert.Equal(manifest.Architecture, loadedManifest.Architecture);
        Assert.Equal(manifest.SourceRevision, loadedManifest.SourceRevision);
        Assert.Equal(manifest.ReleaseChannel, loadedManifest.ReleaseChannel);
        Assert.Equal(manifest.PackageDigest, loadedManifest.PackageDigest);
        Assert.Equal(manifest.InstalledDigest, loadedManifest.InstalledDigest);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
