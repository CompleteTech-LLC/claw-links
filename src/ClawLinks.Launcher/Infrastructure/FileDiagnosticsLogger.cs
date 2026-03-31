using ClawLinks.Launcher.Application;
using System.Text;
using System.Text.Json;

namespace ClawLinks.Launcher.Infrastructure;

public sealed class FileDiagnosticsLogger : IDiagnosticsLogger
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task LogSuccessAsync(AppLayout layout, LinkOpenResult result, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);

        return AppendAsync(
            layout,
            new LauncherDiagnosticsEntry(
                TimestampUtc: DateTimeOffset.UtcNow,
                Outcome: "success",
                AttemptedUrl: result.Url.AbsoluteUri,
                ErrorMessage: null,
                BrowserBundle: result.Diagnostics),
            cancellationToken);
    }

    public Task LogFailureAsync(AppLayout layout, string attemptedUrl, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return AppendAsync(
            layout,
            new LauncherDiagnosticsEntry(
                TimestampUtc: DateTimeOffset.UtcNow,
                Outcome: "failure",
                AttemptedUrl: attemptedUrl,
                ErrorMessage: exception.Message,
                BrowserBundle: null),
            cancellationToken);
    }

    private static async Task AppendAsync(AppLayout layout, LauncherDiagnosticsEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(entry);

        var logDirectory = Path.GetDirectoryName(layout.DiagnosticsLogPath)
            ?? throw new InvalidOperationException("Unable to determine diagnostics log directory.");

        Directory.CreateDirectory(logDirectory);

        var payload = JsonSerializer.Serialize(entry, SerializerOptions);
        await File.AppendAllTextAsync(layout.DiagnosticsLogPath, payload + Environment.NewLine, Encoding.UTF8, cancellationToken);
    }
}
