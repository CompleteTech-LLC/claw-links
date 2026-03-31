namespace ClawLinks.Launcher.Application;

public sealed class LinkOpenService(
    IPathResolver pathResolver,
    IBrowserBundleBootstrapper browserBundleBootstrapper,
    IProfileBootstrapper profileBootstrapper,
    IDiagnosticsLogger diagnosticsLogger,
    IBrowserLauncher browserLauncher)
{
    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly IBrowserBundleBootstrapper _browserBundleBootstrapper = browserBundleBootstrapper;
    private readonly IProfileBootstrapper _profileBootstrapper = profileBootstrapper;
    private readonly IDiagnosticsLogger _diagnosticsLogger = diagnosticsLogger;
    private readonly IBrowserLauncher _browserLauncher = browserLauncher;

    public async Task<LinkOpenResult> OpenAsync(string rawUrl, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawUrl);
        var layout = _pathResolver.Resolve();

        try
        {
            if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var url))
            {
                throw new ArgumentException("The provided value is not a valid absolute URL.", nameof(rawUrl));
            }

            var isSupportedScheme =
                string.Equals(url.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

            if (!isSupportedScheme)
            {
                throw new ArgumentException("Only http and https URLs are supported.", nameof(rawUrl));
            }

            await _profileBootstrapper.BootstrapAsync(layout, cancellationToken);
            var browserBundle = await _browserBundleBootstrapper.BootstrapAsync(layout, cancellationToken);
            var launchRequest = BrowserLaunchRequest.Create(browserBundle.ExecutablePath, layout.ProfileRoot, url);
            await _browserLauncher.LaunchAsync(launchRequest, cancellationToken);

            var result = new LinkOpenResult(url, browserBundle);
            await _diagnosticsLogger.LogSuccessAsync(layout, result, cancellationToken);
            return result;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            await _diagnosticsLogger.LogFailureAsync(layout, rawUrl, ex, cancellationToken);
            throw;
        }
    }
}
