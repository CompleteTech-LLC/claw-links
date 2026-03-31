using ClawLinks.Launcher.Application;
using ClawLinks.Launcher.Infrastructure;
using Xunit;

namespace ClawLinks.Launcher.Tests;

public sealed class WindowsIntegrationTests
{
    [Fact]
    public void DefaultPathResolver_OnWindows_UsesExpectedBundleLayout()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var resolver = new DefaultPathResolver();
        var layout = resolver.Resolve();
        var expectedRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClawLinks");

        Assert.Equal(expectedRoot, layout.AppDataRoot);
        Assert.Equal(Path.Combine(expectedRoot, "browser"), layout.BrowserRoot);
        Assert.Equal(Path.Combine(expectedRoot, "profile"), layout.ProfileRoot);
        Assert.Equal(Path.Combine(expectedRoot, "browser", "bundle-manifest.json"), layout.BundleManifestPath);
    }

    [Fact]
    public void DefaultPathResolver_OnWindows_UsesOverrideWhenSet()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var overrideRoot = Path.Combine(Path.GetTempPath(), "claw-links-tests", Guid.NewGuid().ToString("N"));
        var originalValue = Environment.GetEnvironmentVariable(DefaultPathResolver.AppDataRootOverrideEnvironmentVariable);

        Environment.SetEnvironmentVariable(DefaultPathResolver.AppDataRootOverrideEnvironmentVariable, overrideRoot);

        try
        {
            var resolver = new DefaultPathResolver();
            var layout = resolver.Resolve();
            var expectedRoot = Path.GetFullPath(overrideRoot);

            Assert.Equal(expectedRoot, layout.AppDataRoot);
            Assert.Equal(Path.Combine(expectedRoot, "browser"), layout.BrowserRoot);
            Assert.Equal(Path.Combine(expectedRoot, "profile"), layout.ProfileRoot);
            Assert.Equal(Path.Combine(expectedRoot, "browser", "bundle-manifest.json"), layout.BundleManifestPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable(DefaultPathResolver.AppDataRootOverrideEnvironmentVariable, originalValue);
        }
    }

    [Fact]
    public void ProcessBrowserLauncher_BuildStartInfo_UsesManagedArguments()
    {
        var request = BrowserLaunchRequest.Create(
            @"C:\Users\timot\AppData\Local\ClawLinks\browser\claw-browser.exe",
            @"C:\Users\timot\AppData\Local\ClawLinks\profile",
            new Uri("https://discord.com/channels/1/2/3"));

        var startInfo = ProcessBrowserLauncher.BuildStartInfo(request);

        Assert.Equal(request.BrowserExecutablePath, startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(
            ["--new-instance", "--profile", request.ProfileRoot, request.Url.AbsoluteUri],
            startInfo.ArgumentList.ToArray());
    }
}
