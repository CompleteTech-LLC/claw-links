namespace ClawLinks.Launcher.Application;

public sealed record LauncherDiagnosticsEntry(
    DateTimeOffset TimestampUtc,
    string Outcome,
    string AttemptedUrl,
    string? ErrorMessage,
    BrowserBundleDiagnostics? BrowserBundle);
