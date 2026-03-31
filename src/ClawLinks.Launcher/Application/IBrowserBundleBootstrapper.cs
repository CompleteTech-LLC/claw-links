namespace ClawLinks.Launcher.Application;

public interface IBrowserBundleBootstrapper
{
    Task<BrowserBundle> BootstrapAsync(AppLayout layout, CancellationToken cancellationToken);
}
