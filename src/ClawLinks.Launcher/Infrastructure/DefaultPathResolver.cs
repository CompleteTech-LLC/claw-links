using ClawLinks.Launcher.Application;
using System.Runtime.InteropServices;

namespace ClawLinks.Launcher.Infrastructure;

public sealed class DefaultPathResolver : IPathResolver
{
    public const string AppDataRootOverrideEnvironmentVariable = "CLAW_LINKS_APPDATA_ROOT";

    public AppLayout Resolve()
    {
        var appDataRoot = ResolveAppDataRoot();
        var browserRoot = Path.Combine(appDataRoot, "browser");
        var profileRoot = Path.Combine(appDataRoot, "profile");
        var policyTemplatePath = Path.Combine(AppContext.BaseDirectory, "assets", "firefox", "policies.json");
        var bundleManifestPath = Path.Combine(browserRoot, "bundle-manifest.json");
        var diagnosticsLogPath = Path.Combine(appDataRoot, "logs", "open.ndjson");

        return new AppLayout(
            appDataRoot,
            browserRoot,
            profileRoot,
            policyTemplatePath,
            bundleManifestPath,
            diagnosticsLogPath);
    }

    private static string ResolveAppDataRoot()
    {
        var overrideRoot = Environment.GetEnvironmentVariable(AppDataRootOverrideEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(overrideRoot));
        }

        if (OperatingSystem.IsWindows())
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "ClawLinks");
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", "ClawLinks");
        }

        if (OperatingSystem.IsLinux())
        {
            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (!string.IsNullOrWhiteSpace(xdgDataHome))
            {
                return Path.Combine(xdgDataHome, "claw-links");
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", "claw-links");
        }

        throw new PlatformNotSupportedException($"Unsupported operating system: {RuntimeInformation.OSDescription}");
    }
}
