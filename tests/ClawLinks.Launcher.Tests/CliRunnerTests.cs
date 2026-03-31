using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Cli;
using System.Text;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class CliRunnerTests
{
    [Fact]
    public async Task RunAsync_OpenCommand_PrintsBundleDiagnostics()
    {
        var stdout = new StringWriter(new StringBuilder());
        var stderr = new StringWriter(new StringBuilder());
        var service = new LinkOpenService(
            new TestPathResolver(),
            new TestBrowserBundleBootstrapper(),
            new TestProfileBootstrapper(),
            new TestDiagnosticsLogger(),
            new TestBrowserLauncher());

        var exitCode = await CliRunner.RunAsync(
            ["open", "https://discord.com/channels/1/2/3"],
            service,
            stdout,
            stderr,
            CancellationToken.None);

        var output = stdout.ToString();
        Assert.Equal(0, exitCode);
        Assert.Contains("Bundle Diagnostics:", output, StringComparison.Ordinal);
        Assert.Contains("upstream product line: esr", output, StringComparison.Ordinal);
        Assert.Contains("package digest: sha256:package", output, StringComparison.Ordinal);
        Assert.Contains("installed digest: sha256:installed", output, StringComparison.Ordinal);
        Assert.Equal(string.Empty, stderr.ToString());
    }

    private sealed class TestPathResolver : IPathResolver
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

    private sealed class TestBrowserBundleBootstrapper : IBrowserBundleBootstrapper
    {
        public Task<BrowserBundle> BootstrapAsync(AppLayout layout, CancellationToken cancellationToken)
        {
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

    private sealed class TestProfileBootstrapper : IProfileBootstrapper
    {
        public Task BootstrapAsync(AppLayout layout, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestDiagnosticsLogger : IDiagnosticsLogger
    {
        public Task LogSuccessAsync(AppLayout layout, LinkOpenResult result, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task LogFailureAsync(AppLayout layout, string attemptedUrl, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestBrowserLauncher : IBrowserLauncher
    {
        public Task LaunchAsync(BrowserLaunchRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
