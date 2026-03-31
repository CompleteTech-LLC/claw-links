using ClawLinks.Launcher.Application;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class LinkOpenServiceTests
{
    [Fact]
    public async Task OpenAsync_RejectsNonHttpSchemes()
    {
        var service = new LinkOpenService(
            new FakePathResolver(),
            new FakeBrowserBundleBootstrapper(),
            new FakeProfileBootstrapper(),
            new FakeDiagnosticsLogger(),
            new FakeBrowserLauncher());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.OpenAsync("file:///tmp/test", CancellationToken.None));

        Assert.Contains("Only http and https URLs are supported.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenAsync_BootstrapsProfileBeforeLaunch()
    {
        var tracker = new CallTracker();
        var service = new LinkOpenService(
            new FakePathResolver(),
            new FakeBrowserBundleBootstrapper(tracker),
            new FakeProfileBootstrapper(tracker),
            new FakeDiagnosticsLogger(tracker),
            new FakeBrowserLauncher(tracker));

        await service.OpenAsync("https://discord.com/channels/1/2/3", CancellationToken.None);

        Assert.Equal(["bootstrap", "bundle", "launch", "log-success"], tracker.Calls);
    }

    [Fact]
    public async Task OpenAsync_ReturnsBundleDiagnostics()
    {
        var service = new LinkOpenService(
            new FakePathResolver(),
            new FakeBrowserBundleBootstrapper(),
            new FakeProfileBootstrapper(),
            new FakeDiagnosticsLogger(),
            new FakeBrowserLauncher());

        var result = await service.OpenAsync("https://discord.com/channels/1/2/3", CancellationToken.None);

        Assert.Equal("esr", result.BrowserBundle.Manifest.UpstreamProductLine);
        Assert.Equal("sha256:package", result.BrowserBundle.Manifest.PackageDigest);
        Assert.Equal("sha256:installed", result.BrowserBundle.Manifest.InstalledDigest);
    }

    [Fact]
    public async Task OpenAsync_LogsFailureForInvalidUrl()
    {
        var tracker = new CallTracker();
        var service = new LinkOpenService(
            new FakePathResolver(),
            new FakeBrowserBundleBootstrapper(tracker),
            new FakeProfileBootstrapper(tracker),
            new FakeDiagnosticsLogger(tracker),
            new FakeBrowserLauncher(tracker));

        await Assert.ThrowsAsync<ArgumentException>(() => service.OpenAsync("not-a-url", CancellationToken.None));

        Assert.Equal(["log-failure"], tracker.Calls);
    }

    private sealed class CallTracker
    {
        public List<string> Calls { get; } = [];
    }

    private sealed class FakePathResolver : IPathResolver
    {
        public AppLayout Resolve()
        {
            return new AppLayout(
                AppDataRoot: "/tmp/claw-links",
                BrowserRoot: "/tmp/claw-links/browser",
                ProfileRoot: "/tmp/claw-links/profile",
                PolicyTemplatePath: "/tmp/claw-links/policies.json",
                BundleManifestPath: "/tmp/claw-links/browser/bundle-manifest.json",
                DiagnosticsLogPath: "/tmp/claw-links/logs/open.ndjson");
        }
    }

    private sealed class FakeBrowserBundleBootstrapper(CallTracker? tracker = null) : IBrowserBundleBootstrapper
    {
        public Task<BrowserBundle> BootstrapAsync(AppLayout layout, CancellationToken cancellationToken)
        {
            tracker?.Calls.Add("bundle");
            return Task.FromResult(new BrowserBundle(
                "/tmp/claw-links/browser/firefox",
                "/tmp/claw-links/browser/distribution/policies.json",
                new BrowserBundleManifest(
                    SchemaVersion: 1,
                    BundleId: "claw-browser-linux",
                    BundleVersion: "1.0.0",
                    Platform: "linux-x64",
                    Architecture: "x64",
                    UpstreamProductLine: "esr",
                    SourceRevision: "firefox-128.0esr-build1",
                    ReleaseChannel: "production",
                    PackageDigest: "sha256:package",
                    InstalledDigest: "sha256:installed",
                    DisplayName: "Claw Browser",
                    EntryExecutableRelativePath: "bundles/1.0.0/claw-browser",
                    PolicyRelativePath: "bundles/1.0.0/distribution/policies.json",
                    SupportFiles:
                    [
                        new BrowserSupportFile("bundles/1.0.0/defaults/pref/local-settings.js", "autoconfig-loader"),
                        new BrowserSupportFile("bundles/1.0.0/mozilla.cfg", "autoconfig-config")
                    ]),
                ["/tmp/claw-links/browser/defaults/pref/local-settings.js", "/tmp/claw-links/browser/mozilla.cfg"]));
        }
    }

    private sealed class FakeProfileBootstrapper(CallTracker? tracker = null) : IProfileBootstrapper
    {
        public Task BootstrapAsync(AppLayout layout, CancellationToken cancellationToken)
        {
            tracker?.Calls.Add("bootstrap");
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDiagnosticsLogger(CallTracker? tracker = null) : IDiagnosticsLogger
    {
        public Task LogSuccessAsync(AppLayout layout, LinkOpenResult result, CancellationToken cancellationToken)
        {
            tracker?.Calls.Add("log-success");
            return Task.CompletedTask;
        }

        public Task LogFailureAsync(AppLayout layout, string attemptedUrl, Exception exception, CancellationToken cancellationToken)
        {
            tracker?.Calls.Add("log-failure");
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBrowserLauncher(CallTracker? tracker = null) : IBrowserLauncher
    {
        public Task LaunchAsync(BrowserLaunchRequest request, CancellationToken cancellationToken)
        {
            tracker?.Calls.Add("launch");
            return Task.CompletedTask;
        }
    }
}
