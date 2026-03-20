namespace ProfileIpSwitcher.Models;

public class AppSettings
{
    public bool StartWithWindows { get; set; }

    public bool MinimizeToTrayInsteadOfClose { get; set; }

    public bool CheckForUpdatesOnStartup { get; set; }

    /// <summary>Keine Rückfrage beim Doppelklick auf ein Profil (direkt anwenden).</summary>
    public bool SkipDoubleClickApplyConfirmation { get; set; }

    /// <summary>Basis-URL oder JSON-Endpunkt für Update-Checks (Platzhalter).</summary>
    public string UpdateCheckUrl { get; set; } = "https://github.com/example/ProfileIpSwitcher/releases/latest";

    public string GitHubReleasesUrl { get; set; } = "https://github.com/example/ProfileIpSwitcher/releases";
}
