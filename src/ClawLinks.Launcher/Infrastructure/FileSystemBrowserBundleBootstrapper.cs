using ClawLinks.Launcher.Application;

namespace ClawLinks.Launcher.Infrastructure;

public sealed class FileSystemBrowserBundleBootstrapper : IBrowserBundleBootstrapper
{
    private const int SupportedManifestSchemaVersion = 1;

    public async Task<BrowserBundle> BootstrapAsync(AppLayout layout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(layout);

        if (!File.Exists(layout.PolicyTemplatePath))
        {
            throw new IOException($"Policy template not found at '{layout.PolicyTemplatePath}'.");
        }

        Directory.CreateDirectory(layout.BrowserRoot);

        if (!File.Exists(layout.BundleManifestPath))
        {
            throw new InvalidOperationException(
                "Managed browser bundle manifest not found. Run the Windows packaging stub or install a packaged browser bundle first.");
        }

        var manifest = await BrowserBundleManifestStore.LoadAsync(layout.BundleManifestPath, cancellationToken);
        ValidateManifest(manifest);

        var executablePath = ResolveBundledPath(layout.BrowserRoot, manifest.EntryExecutableRelativePath);
        var policyInstallPath = ResolveBundledPath(layout.BrowserRoot, manifest.PolicyRelativePath);
        var supportFilePaths = manifest.SupportFiles
            .Select(supportFile => (supportFile.Role, Path: ResolveBundledPath(layout.BrowserRoot, supportFile.RelativePath)))
            .ToArray();

        var missingSupportFiles = supportFilePaths
            .Where(supportFile => !File.Exists(supportFile.Path))
            .Select(supportFile => $"{supportFile.Role}: {supportFile.Path}")
            .ToArray();

        if (missingSupportFiles.Length > 0)
        {
            throw new InvalidOperationException(
                "Managed browser support file(s) missing:" + Environment.NewLine +
                string.Join(Environment.NewLine, missingSupportFiles.Select(path => $"  - {path}")));
        }

        var policyDirectory = Path.GetDirectoryName(policyInstallPath)
            ?? throw new InvalidOperationException("Unable to determine the browser policy directory.");

        Directory.CreateDirectory(policyDirectory);
        await CopyIfChangedAsync(layout.PolicyTemplatePath, policyInstallPath, cancellationToken);

        if (!File.Exists(executablePath))
        {
            throw new InvalidOperationException(
                $"Managed browser bundle executable not found at '{executablePath}'.");
        }

        return new BrowserBundle(
            executablePath,
            policyInstallPath,
            manifest,
            supportFilePaths.Select(supportFile => supportFile.Path).ToArray());
    }

    private static void ValidateManifest(BrowserBundleManifest manifest)
    {
        if (manifest.SchemaVersion != SupportedManifestSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported browser bundle manifest schema version '{manifest.SchemaVersion}'.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.BundleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.BundleVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.Platform);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.Architecture);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.UpstreamProductLine);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.SourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.ReleaseChannel);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.PackageDigest);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.InstalledDigest);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.EntryExecutableRelativePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.PolicyRelativePath);
    }

    private static string ResolveBundledPath(string browserRoot, string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(browserRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException("Browser bundle manifest paths must be relative to the browser root.");
        }

        var browserRootFullPath = Path.GetFullPath(browserRoot);
        var candidatePath = Path.GetFullPath(Path.Combine(browserRootFullPath, relativePath));

        if (!candidatePath.StartsWith(browserRootFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Browser bundle manifest paths must remain inside the browser root.");
        }

        return candidatePath;
    }

    private static async Task CopyIfChangedAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        var sourceBytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
        if (File.Exists(destinationPath))
        {
            var destinationBytes = await File.ReadAllBytesAsync(destinationPath, cancellationToken);
            if (sourceBytes.AsSpan().SequenceEqual(destinationBytes))
            {
                return;
            }
        }

        await File.WriteAllBytesAsync(destinationPath, sourceBytes, cancellationToken);
    }
}
