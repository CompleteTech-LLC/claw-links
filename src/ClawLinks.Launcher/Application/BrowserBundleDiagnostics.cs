namespace ClawLinks.Launcher.Application;

public sealed record BrowserBundleDiagnostics(
    string BundleId,
    string BundleVersion,
    string UpstreamProductLine,
    string SourceRevision,
    string ReleaseChannel,
    string PackageDigest,
    string InstalledDigest);
