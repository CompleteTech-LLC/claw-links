namespace ClawLinks.Launcher.Application;

public interface IDiagnosticsLogger
{
    Task LogSuccessAsync(AppLayout layout, LinkOpenResult result, CancellationToken cancellationToken);

    Task LogFailureAsync(AppLayout layout, string attemptedUrl, Exception exception, CancellationToken cancellationToken);
}
