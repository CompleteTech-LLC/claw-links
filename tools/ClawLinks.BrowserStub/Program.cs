var markerPath = Environment.GetEnvironmentVariable("CLAW_LINKS_SMOKE_MARKER_PATH");

if (!string.IsNullOrWhiteSpace(markerPath))
{
    var markerDirectory = Path.GetDirectoryName(markerPath);
    if (!string.IsNullOrWhiteSpace(markerDirectory))
    {
        Directory.CreateDirectory(markerDirectory);
    }

    var markerContent = $"stub launched at {DateTimeOffset.UtcNow:O}{Environment.NewLine}";
    await File.AppendAllTextAsync(markerPath, markerContent);
}

Console.Error.WriteLine("This is the Claw Links browser packaging stub, not a production browser build.");
Console.Error.WriteLine("Replace this stub with a self-built Firefox-derived runtime before shipping.");
return 1;
