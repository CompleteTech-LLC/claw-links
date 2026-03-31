using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Infrastructure;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class FileSystemBrowserBundleBootstrapperTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "claw-links-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task BootstrapAsync_CopiesPolicyIntoManifestLayout()
    {
        Directory.CreateDirectory(_tempRoot);

        var browserRoot = Path.Combine(_tempRoot, "browser");
        var bundleVersion = "1.2.3";
        var bundleRoot = Path.Combine(browserRoot, "bundles", bundleVersion);
        var executablePath = Path.Combine(bundleRoot, "claw-browser.exe");
        Directory.CreateDirectory(Path.Combine(bundleRoot, "defaults", "pref"));
        await File.WriteAllTextAsync(executablePath, "stub", CancellationToken.None);
        await File.WriteAllTextAsync(Path.Combine(bundleRoot, "defaults", "pref", "local-settings.js"), "pref('x', 1);", CancellationToken.None);
        await File.WriteAllTextAsync(Path.Combine(bundleRoot, "mozilla.cfg"), "// cfg", CancellationToken.None);

        var policyTemplatePath = Path.Combine(_tempRoot, "policies.json");
        await File.WriteAllTextAsync(policyTemplatePath, """{ "policies": { "DisableTelemetry": true } }""", CancellationToken.None);

        var manifest = new BrowserBundleManifest(
            SchemaVersion: 1,
            BundleId: "claw-browser-windows-x64",
            BundleVersion: bundleVersion,
            Platform: "windows-x64",
            Architecture: "x64",
            UpstreamProductLine: "stable",
            SourceRevision: "mozilla-central@abc123",
            ReleaseChannel: "production",
            PackageDigest: "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            InstalledDigest: "sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
            DisplayName: "Claw Browser",
            EntryExecutableRelativePath: $"bundles/{bundleVersion}/claw-browser.exe",
            PolicyRelativePath: $"bundles/{bundleVersion}/distribution/policies.json",
            SupportFiles:
            [
                new BrowserSupportFile($"bundles/{bundleVersion}/defaults/pref/local-settings.js", "autoconfig-loader"),
                new BrowserSupportFile($"bundles/{bundleVersion}/mozilla.cfg", "autoconfig-config")
            ]);

        var manifestPath = Path.Combine(browserRoot, "bundle-manifest.json");
        await BrowserBundleManifestStore.SaveAsync(manifestPath, manifest, CancellationToken.None);

        var layout = new AppLayout(
            AppDataRoot: _tempRoot,
            BrowserRoot: browserRoot,
            ProfileRoot: Path.Combine(_tempRoot, "profile"),
            PolicyTemplatePath: policyTemplatePath,
            BundleManifestPath: manifestPath,
            DiagnosticsLogPath: Path.Combine(_tempRoot, "logs", "open.ndjson"));

        var bootstrapper = new FileSystemBrowserBundleBootstrapper();
        var bundle = await bootstrapper.BootstrapAsync(layout, CancellationToken.None);

        Assert.Equal(executablePath, bundle.ExecutablePath);
        Assert.True(File.Exists(bundle.PolicyInstallPath));
        Assert.Equal(await File.ReadAllTextAsync(policyTemplatePath), await File.ReadAllTextAsync(bundle.PolicyInstallPath));

        var persistedManifest = await BrowserBundleManifestStore.LoadAsync(manifestPath, CancellationToken.None);
        Assert.Equal("stable", persistedManifest.UpstreamProductLine);
        Assert.Equal("x64", persistedManifest.Architecture);
        Assert.Equal("mozilla-central@abc123", persistedManifest.SourceRevision);
        Assert.Equal("production", persistedManifest.ReleaseChannel);
        Assert.Equal("sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", persistedManifest.PackageDigest);
        Assert.Equal("sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc", persistedManifest.InstalledDigest);
    }

    [Fact]
    public async Task BootstrapAsync_MissingSupportFilesFailsClearly()
    {
        Directory.CreateDirectory(_tempRoot);

        var browserRoot = Path.Combine(_tempRoot, "browser");
        var bundleVersion = "4.5.6";
        var bundleRoot = Path.Combine(browserRoot, "bundles", bundleVersion);
        Directory.CreateDirectory(bundleRoot);
        await File.WriteAllTextAsync(Path.Combine(bundleRoot, "claw-browser.exe"), "stub", CancellationToken.None);

        var policyTemplatePath = Path.Combine(_tempRoot, "policies.json");
        await File.WriteAllTextAsync(policyTemplatePath, """{ "policies": { "DisableTelemetry": true } }""", CancellationToken.None);

        var manifestPath = Path.Combine(browserRoot, "bundle-manifest.json");
        await BrowserBundleManifestStore.SaveAsync(
            manifestPath,
            new BrowserBundleManifest(
                SchemaVersion: 1,
                BundleId: "claw-browser-windows-x64",
                BundleVersion: bundleVersion,
                Platform: "windows-x64",
                Architecture: "x64",
                UpstreamProductLine: "stable",
                SourceRevision: "mozilla-central@missing-support",
                ReleaseChannel: "production",
                PackageDigest: "sha256:eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee",
                InstalledDigest: "sha256:ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff",
                DisplayName: "Claw Browser",
                EntryExecutableRelativePath: $"bundles/{bundleVersion}/claw-browser.exe",
                PolicyRelativePath: $"bundles/{bundleVersion}/distribution/policies.json",
                SupportFiles:
                [
                    new BrowserSupportFile($"bundles/{bundleVersion}/defaults/pref/local-settings.js", "autoconfig-loader"),
                    new BrowserSupportFile($"bundles/{bundleVersion}/mozilla.cfg", "autoconfig-config")
                ]),
            CancellationToken.None);

        var bootstrapper = new FileSystemBrowserBundleBootstrapper();
        var layout = new AppLayout(
            AppDataRoot: _tempRoot,
            BrowserRoot: browserRoot,
            ProfileRoot: Path.Combine(_tempRoot, "profile"),
            PolicyTemplatePath: policyTemplatePath,
            BundleManifestPath: manifestPath,
            DiagnosticsLogPath: Path.Combine(_tempRoot, "logs", "open.ndjson"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => bootstrapper.BootstrapAsync(layout, CancellationToken.None));

        Assert.Contains("Managed browser support file(s) missing:", exception.Message, StringComparison.Ordinal);
        Assert.Contains("autoconfig-loader", exception.Message, StringComparison.Ordinal);
        Assert.Contains("autoconfig-config", exception.Message, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
