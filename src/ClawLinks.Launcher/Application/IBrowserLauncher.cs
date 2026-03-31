namespace ClawLinks.Launcher.Application;

public interface IBrowserLauncher
{
    Task LaunchAsync(BrowserLaunchRequest request, CancellationToken cancellationToken);
}
