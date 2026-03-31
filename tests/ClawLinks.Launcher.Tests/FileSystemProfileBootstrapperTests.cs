using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Infrastructure;
using System.Text.Json;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class FileSystemProfileBootstrapperTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "claw-links-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task BootstrapAsync_WritesManagedProfileFiles()
    {
        Directory.CreateDirectory(_tempRoot);

        var layout = new AppLayout(
            AppDataRoot: _tempRoot,
            BrowserRoot: Path.Combine(_tempRoot, "browser"),
            ProfileRoot: Path.Combine(_tempRoot, "profile"),
            PolicyTemplatePath: Path.Combine(_tempRoot, "policies.json"),
            BundleManifestPath: Path.Combine(_tempRoot, "browser", "bundle-manifest.json"),
            DiagnosticsLogPath: Path.Combine(_tempRoot, "logs", "open.ndjson"));

        var bootstrapper = new FileSystemProfileBootstrapper();
        await bootstrapper.BootstrapAsync(layout, CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(layout.ProfileRoot, ".claw-links-profile")));
        Assert.True(File.Exists(Path.Combine(layout.ProfileRoot, ".claw-links-first-run-complete")));
        Assert.True(File.Exists(Path.Combine(layout.ProfileRoot, "user.js")));
        Assert.True(File.Exists(Path.Combine(layout.ProfileRoot, "claw-links-profile-state.json")));

        var userJs = await File.ReadAllTextAsync(Path.Combine(layout.ProfileRoot, "user.js"));
        Assert.Contains("""user_pref("browser.startup.homepage_override.mstone", "ignore");""", userJs, StringComparison.Ordinal);
        Assert.Contains("""user_pref("signon.rememberSignons", false);""", userJs, StringComparison.Ordinal);

        await using var stateStream = File.OpenRead(Path.Combine(layout.ProfileRoot, "claw-links-profile-state.json"));
        using var profileState = await JsonDocument.ParseAsync(stateStream);
        Assert.Equal(1, profileState.RootElement.GetProperty("SchemaVersion").GetInt32());
        Assert.Contains(
            "privacy.sanitize.sanitizeOnShutdown",
            profileState.RootElement.GetProperty("PolicyLockedPreferences").EnumerateArray().Select(element => element.GetString()));
        Assert.Contains(
            "browser.startup.homepage",
            profileState.RootElement.GetProperty("BrowserLockedPreferences").EnumerateArray().Select(element => element.GetString()));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
