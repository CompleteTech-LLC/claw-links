namespace ClawLinks.Launcher.Application;

public sealed record BrowserBundle(
    string ExecutablePath,
    string PolicyInstallPath,
    BrowserBundleManifest Manifest,
    IReadOnlyList<string> SupportFilePaths);
