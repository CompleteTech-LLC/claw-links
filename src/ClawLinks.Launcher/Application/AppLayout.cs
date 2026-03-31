namespace ClawLinks.Launcher.Application;

public sealed record AppLayout(
    string AppDataRoot,
    string BrowserRoot,
    string ProfileRoot,
    string PolicyTemplatePath,
    string BundleManifestPath,
    string DiagnosticsLogPath);
