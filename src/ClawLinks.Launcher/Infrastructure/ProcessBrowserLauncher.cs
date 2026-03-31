using ClawLinks.Launcher.Application;
using System.Diagnostics;

namespace ClawLinks.Launcher.Infrastructure;

public sealed class ProcessBrowserLauncher(IProcessStarter? processStarter = null) : IBrowserLauncher
{
    private readonly IProcessStarter _processStarter = processStarter ?? new SystemProcessStarter();

    public Task LaunchAsync(BrowserLaunchRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(request.BrowserExecutablePath))
        {
            throw new InvalidOperationException($"Managed browser executable not found at '{request.BrowserExecutablePath}'.");
        }

        var startInfo = BuildStartInfo(request);
        _processStarter.Start(startInfo);
        return Task.CompletedTask;
    }

    public static ProcessStartInfo BuildStartInfo(BrowserLaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.BrowserExecutablePath,
            UseShellExecute = false
        };

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }
}
