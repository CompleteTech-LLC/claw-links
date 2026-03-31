using System.Text.Json;

namespace ClawLinks.Launcher.Application;

public static class BrowserBundleManifestStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static async Task<BrowserBundleManifest> LoadAsync(string manifestPath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<BrowserBundleManifest>(stream, SerializerOptions, cancellationToken);
        return manifest ?? throw new InvalidOperationException($"Bundle manifest at '{manifestPath}' could not be read.");
    }

    public static async Task SaveAsync(string manifestPath, BrowserBundleManifest manifest, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);
        ArgumentNullException.ThrowIfNull(manifest);

        var manifestDirectory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrWhiteSpace(manifestDirectory))
        {
            Directory.CreateDirectory(manifestDirectory);
        }

        await using var stream = File.Create(manifestPath);
        await JsonSerializer.SerializeAsync(stream, manifest, SerializerOptions, cancellationToken);
    }
}
