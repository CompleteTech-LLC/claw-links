namespace ClawLinks.Launcher.Application;

public sealed record LinkOpenResult(Uri Url, BrowserBundle BrowserBundle)
{
    public BrowserBundleDiagnostics Diagnostics => new(
        BrowserBundle.Manifest.BundleId,
        BrowserBundle.Manifest.BundleVersion,
        BrowserBundle.Manifest.UpstreamProductLine,
        BrowserBundle.Manifest.SourceRevision,
        BrowserBundle.Manifest.ReleaseChannel,
        BrowserBundle.Manifest.PackageDigest,
        BrowserBundle.Manifest.InstalledDigest);
}
