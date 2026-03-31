using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Infrastructure;
using System.Text.Json;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class FileDiagnosticsLoggerTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "claw-links-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LogSuccessAsync_AppendsStructuredEntry()
    {
        Directory.CreateDirectory(_tempRoot);

        var layout = new AppLayout(
            AppDataRoot: _tempRoot,
            BrowserRoot: Path.Combine(_tempRoot, "browser"),
            ProfileRoot: Path.Combine(_tempRoot, "profile"),
            PolicyTemplatePath: Path.Combine(_tempRoot, "policies.json"),
            BundleManifestPath: Path.Combine(_tempRoot, "browser", "bundle-manifest.json"),
            DiagnosticsLogPath: Path.Combine(_tempRoot, "logs", "open.ndjson"));

        var logger = new FileDiagnosticsLogger();
        var result = new LinkOpenResult(
            new Uri("https://discord.com/channels/1/2/3"),
            new BrowserBundle(
                ExecutablePath: Path.Combine(_tempRoot, "browser", "claw-browser.exe"),
                PolicyInstallPath: Path.Combine(_tempRoot, "browser", "distribution", "policies.json"),
                Manifest: new BrowserBundleManifest(
                    SchemaVersion: 1,
                    BundleId: "claw-browser-windows-x64",
                    BundleVersion: "1.0.0",
                    Platform: "windows-x64",
                    Architecture: "x64",
                    UpstreamProductLine: "esr",
                    SourceRevision: "firefox-128.0esr-build1",
                    ReleaseChannel: "production",
                    PackageDigest: "sha256:package",
                    InstalledDigest: "sha256:installed",
                    DisplayName: "Claw Browser",
                    EntryExecutableRelativePath: "bundles/1.0.0/claw-browser.exe",
                    PolicyRelativePath: "bundles/1.0.0/distribution/policies.json",
                    SupportFiles:
                    [
                        new BrowserSupportFile("bundles/1.0.0/defaults/pref/local-settings.js", "autoconfig-loader"),
                        new BrowserSupportFile("bundles/1.0.0/mozilla.cfg", "autoconfig-config")
                    ]),
                SupportFilePaths: [Path.Combine(_tempRoot, "browser", "mozilla.cfg")]));

        await logger.LogSuccessAsync(layout, result, CancellationToken.None);

        var logLines = await File.ReadAllLinesAsync(layout.DiagnosticsLogPath, CancellationToken.None);
        Assert.Single(logLines);

        using var document = JsonDocument.Parse(logLines[0]);
        Assert.Equal("success", document.RootElement.GetProperty("outcome").GetString());
        Assert.Equal("esr", document.RootElement.GetProperty("browserBundle").GetProperty("upstreamProductLine").GetString());
        Assert.Equal("sha256:package", document.RootElement.GetProperty("browserBundle").GetProperty("packageDigest").GetString());
    }

    [Fact]
    public async Task LogFailureAsync_AppendsFailureEntry()
    {
        Directory.CreateDirectory(_tempRoot);

        var layout = new AppLayout(
            AppDataRoot: _tempRoot,
            BrowserRoot: Path.Combine(_tempRoot, "browser"),
            ProfileRoot: Path.Combine(_tempRoot, "profile"),
            PolicyTemplatePath: Path.Combine(_tempRoot, "policies.json"),
            BundleManifestPath: Path.Combine(_tempRoot, "browser", "bundle-manifest.json"),
            DiagnosticsLogPath: Path.Combine(_tempRoot, "logs", "open.ndjson"));

        var logger = new FileDiagnosticsLogger();
        await logger.LogFailureAsync(layout, "not-a-url", new InvalidOperationException("boom"), CancellationToken.None);

        var logLines = await File.ReadAllLinesAsync(layout.DiagnosticsLogPath, CancellationToken.None);
        Assert.Single(logLines);

        using var document = JsonDocument.Parse(logLines[0]);
        Assert.Equal("failure", document.RootElement.GetProperty("outcome").GetString());
        Assert.Equal("not-a-url", document.RootElement.GetProperty("attemptedUrl").GetString());
        Assert.Equal("boom", document.RootElement.GetProperty("errorMessage").GetString());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
