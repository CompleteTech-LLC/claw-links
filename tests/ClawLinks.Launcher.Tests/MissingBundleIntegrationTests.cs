using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Infrastructure;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class MissingBundleIntegrationTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "claw-links-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task OpenAsync_MissingExecutableStillCreatesProfileStateAndCopiesPolicy()
    {
        Directory.CreateDirectory(_tempRoot);

        var browserRoot = Path.Combine(_tempRoot, "browser");
        var profileRoot = Path.Combine(_tempRoot, "profile");
        var bundleVersion = "9.9.9-missing";
        var bundleRoot = Path.Combine(browserRoot, "bundles", bundleVersion);
        Directory.CreateDirectory(Path.Combine(bundleRoot, "defaults", "pref"));
        await File.WriteAllTextAsync(Path.Combine(bundleRoot, "defaults", "pref", "local-settings.js"), "pref('x', 1);", CancellationToken.None);
        await File.WriteAllTextAsync(Path.Combine(bundleRoot, "mozilla.cfg"), "// cfg", CancellationToken.None);
        var manifest = new BrowserBundleManifest(
            SchemaVersion: 1,
            BundleId: "claw-browser-windows-x64",
            BundleVersion: bundleVersion,
            Platform: "windows-x64",
            Architecture: "x64",
            UpstreamProductLine: "stable",
            SourceRevision: "mozilla-central@missing123",
            ReleaseChannel: "staging",
            PackageDigest: "sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
            InstalledDigest: "sha256:dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
            DisplayName: "Claw Browser Missing Bundle Test",
            EntryExecutableRelativePath: $"bundles/{bundleVersion}/claw-browser.exe",
            PolicyRelativePath: $"bundles/{bundleVersion}/distribution/policies.json",
            SupportFiles:
            [
                new BrowserSupportFile($"bundles/{bundleVersion}/defaults/pref/local-settings.js", "autoconfig-loader"),
                new BrowserSupportFile($"bundles/{bundleVersion}/mozilla.cfg", "autoconfig-config")
            ]);

        var manifestPath = Path.Combine(browserRoot, "bundle-manifest.json");
        Directory.CreateDirectory(browserRoot);
        await BrowserBundleManifestStore.SaveAsync(manifestPath, manifest, CancellationToken.None);

        var policyTemplatePath = Path.Combine(_tempRoot, "policies.json");
        await File.WriteAllTextAsync(policyTemplatePath, """{ "policies": { "DisableTelemetry": true } }""", CancellationToken.None);

        var service = new LinkOpenService(
            new TestPathResolver(new AppLayout(
                AppDataRoot: _tempRoot,
                BrowserRoot: browserRoot,
                ProfileRoot: profileRoot,
                PolicyTemplatePath: policyTemplatePath,
                BundleManifestPath: manifestPath,
                DiagnosticsLogPath: Path.Combine(_tempRoot, "logs", "open.ndjson"))),
            new FileSystemBrowserBundleBootstrapper(),
            new FileSystemProfileBootstrapper(),
            new FileDiagnosticsLogger(),
            new ProcessBrowserLauncher(new NoOpProcessStarter()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.OpenAsync("https://discord.com/channels/1/2/3", CancellationToken.None));

        Assert.Contains("Managed browser bundle executable not found", exception.Message, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(profileRoot, "claw-links-profile-state.json")));
        Assert.True(File.Exists(Path.Combine(browserRoot, "bundles", bundleVersion, "distribution", "policies.json")));
        Assert.True(File.Exists(Path.Combine(_tempRoot, "logs", "open.ndjson")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private sealed class TestPathResolver(AppLayout layout) : IPathResolver
    {
        public AppLayout Resolve() => layout;
    }

    private sealed class NoOpProcessStarter : IProcessStarter
    {
        public void Start(System.Diagnostics.ProcessStartInfo startInfo)
        {
        }
    }
}
