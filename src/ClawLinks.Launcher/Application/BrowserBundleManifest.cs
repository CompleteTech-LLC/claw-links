namespace ClawLinks.Launcher.Application;

public sealed record BrowserBundleManifest(
    int SchemaVersion,
    string BundleId,
    string BundleVersion,
    string Platform,
    string Architecture,
    string UpstreamProductLine,
    string SourceRevision,
    string ReleaseChannel,
    string PackageDigest,
    string InstalledDigest,
    string DisplayName,
    string EntryExecutableRelativePath,
    string PolicyRelativePath,
    IReadOnlyList<BrowserSupportFile> SupportFiles);
