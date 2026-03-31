namespace ClawLinks.Launcher.Application;

public sealed record BrowserLaunchRequest(
    string BrowserExecutablePath,
    string ProfileRoot,
    Uri Url,
    IReadOnlyList<string> Arguments)
{
    public static BrowserLaunchRequest Create(string browserExecutablePath, string profileRoot, Uri url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(browserExecutablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileRoot);
        ArgumentNullException.ThrowIfNull(url);

        return new BrowserLaunchRequest(
            browserExecutablePath,
            profileRoot,
            url,
            ["--new-instance", "--profile", profileRoot, url.AbsoluteUri]);
    }
}
