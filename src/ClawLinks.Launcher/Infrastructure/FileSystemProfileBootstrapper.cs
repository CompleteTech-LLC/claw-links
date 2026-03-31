using ClawLinks.Launcher.Application;
using System.Text.Json;
using System.Text;

namespace ClawLinks.Launcher.Infrastructure;

public sealed class FileSystemProfileBootstrapper : IProfileBootstrapper
{
    private static readonly string UserPreferences = """
        user_pref("app.normandy.enabled", false);
        user_pref("browser.bookmarks.restore_default_bookmarks", false);
        user_pref("browser.download.panel.shown", true);
        user_pref("browser.rights.3.shown", true);
        user_pref("browser.startup.homepage_override.mstone", "ignore");
        user_pref("datareporting.policy.dataSubmissionEnabled", false);
        user_pref("datareporting.policy.dataSubmissionPolicyAccepted", true);
        user_pref("extensions.autoDisableScopes", 15);
        user_pref("signon.rememberSignons", false);
        user_pref("toolkit.telemetry.reportingpolicy.firstRun", false);
        """;

    private static readonly string[] PolicyLockedPreferences =
    [
        "permissions.default.camera",
        "permissions.default.desktop-notification",
        "permissions.default.geo",
        "permissions.default.microphone",
        "privacy.sanitize.sanitizeOnShutdown",
        "privacy.trackingprotection.enabled"
    ];

    private static readonly string[] BrowserLockedPreferences =
    [
        "browser.aboutwelcome.enabled",
        "browser.newtabpage.enabled",
        "browser.shell.checkDefaultBrowser",
        "browser.startup.homepage",
        "browser.startup.page"
    ];

    public async Task BootstrapAsync(AppLayout layout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(layout);

        Directory.CreateDirectory(layout.AppDataRoot);
        Directory.CreateDirectory(layout.ProfileRoot);

        var profileMarkerPath = Path.Combine(layout.ProfileRoot, ".claw-links-profile");
        if (!File.Exists(profileMarkerPath))
        {
            await File.WriteAllTextAsync(
                profileMarkerPath,
                "Managed profile created by Claw Links.",
                Encoding.UTF8,
                cancellationToken);
        }

        var userJsPath = Path.Combine(layout.ProfileRoot, "user.js");
        await File.WriteAllTextAsync(userJsPath, UserPreferences + Environment.NewLine, Encoding.UTF8, cancellationToken);

        var firstRunPath = Path.Combine(layout.ProfileRoot, ".claw-links-first-run-complete");
        if (!File.Exists(firstRunPath))
        {
            await File.WriteAllTextAsync(firstRunPath, DateTimeOffset.UtcNow.ToString("O"), Encoding.UTF8, cancellationToken);
        }

        var profileStatePath = Path.Combine(layout.ProfileRoot, "claw-links-profile-state.json");
        var profileState = new ProfileBootstrapState(
            SchemaVersion: 1,
            ManagedBy: "Claw Links",
            CreatedMarkerPath: profileMarkerPath,
            FirstRunMarkerPath: firstRunPath,
            ManagedFiles:
            [
                "user.js",
                ".claw-links-profile",
                ".claw-links-first-run-complete",
                "claw-links-profile-state.json"
            ],
            PolicyLockedPreferences,
            BrowserLockedPreferences);

        await using var profileStateStream = File.Create(profileStatePath);
        await JsonSerializer.SerializeAsync(profileStateStream, profileState, cancellationToken: cancellationToken);
    }

    private sealed record ProfileBootstrapState(
        int SchemaVersion,
        string ManagedBy,
        string CreatedMarkerPath,
        string FirstRunMarkerPath,
        IReadOnlyList<string> ManagedFiles,
        IReadOnlyList<string> PolicyLockedPreferences,
        IReadOnlyList<string> BrowserLockedPreferences);
}
