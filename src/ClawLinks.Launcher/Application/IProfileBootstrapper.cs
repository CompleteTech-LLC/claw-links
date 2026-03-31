namespace ClawLinks.Launcher.Application;

public interface IProfileBootstrapper
{
    Task BootstrapAsync(AppLayout layout, CancellationToken cancellationToken);
}
