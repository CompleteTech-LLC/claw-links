using ClawLinks.Launcher.Application;

namespace ClawLinks.Launcher.Cli;

public static class CliRunner
{
    public static async Task<int> RunAsync(
        string[] args,
        LinkOpenService service,
        TextWriter stdout,
        TextWriter stderr,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);

        if (args.Length == 0 || IsHelp(args[0]))
        {
            WriteUsage(stdout);
            return 0;
        }

        if (!string.Equals(args[0], "open", StringComparison.OrdinalIgnoreCase))
        {
            await stderr.WriteLineAsync($"Unknown command: {args[0]}");
            WriteUsage(stderr);
            return 1;
        }

        if (args.Length != 2)
        {
            await stderr.WriteLineAsync("The open command requires exactly one URL argument.");
            WriteUsage(stderr);
            return 1;
        }

        try
        {
            var result = await service.OpenAsync(args[1], cancellationToken);
            await stdout.WriteLineAsync($"Opening {args[1]} in the managed browser lane.");
            await WriteDiagnosticsAsync(stdout, result);
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
        {
            await stderr.WriteLineAsync(ex.Message);
            return 1;
        }
    }

    private static bool IsHelp(string value)
    {
        return value is "-h" or "--help" or "help";
    }

    private static void WriteUsage(TextWriter writer)
    {
        writer.WriteLine("Usage:");
        writer.WriteLine("  claw-links open <url>");
        writer.WriteLine();
        writer.WriteLine("Commands:");
        writer.WriteLine("  open <url>    Open an http/https URL in the managed browser lane.");
    }

    private static async Task WriteDiagnosticsAsync(TextWriter writer, LinkOpenResult result)
    {
        var manifest = result.BrowserBundle.Manifest;

        await writer.WriteLineAsync("Bundle Diagnostics:");
        await writer.WriteLineAsync($"  bundle id: {manifest.BundleId}");
        await writer.WriteLineAsync($"  bundle version: {manifest.BundleVersion}");
        await writer.WriteLineAsync($"  upstream product line: {manifest.UpstreamProductLine}");
        await writer.WriteLineAsync($"  source revision: {manifest.SourceRevision}");
        await writer.WriteLineAsync($"  release channel: {manifest.ReleaseChannel}");
        await writer.WriteLineAsync($"  package digest: {manifest.PackageDigest}");
        await writer.WriteLineAsync($"  installed digest: {manifest.InstalledDigest}");
    }
}
