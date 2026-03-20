namespace ProfileIpSwitcher.Models;

public class AppSettings
{
    public bool StartWithWindows { get; set; }

    public bool MinimizeToTrayInsteadOfClose { get; set; }

    public bool CheckForUpdatesOnStartup { get; set; }

    /// <summary>Keine Rückfrage beim Doppelklick auf ein Profil (direkt anwenden).</summary>
    public bool SkipDoubleClickApplyConfirmation { get; set; }

    /// <summary>GitHub-Repo-URL (z. B. https://github.com/Benutzer/IP-Changer) oder direkte JSON-URL für version.json.</summary>
    public string UpdateCheckUrl { get; set; } = "https://github.com/zPoche/IP-Changer";

    public string GitHubReleasesUrl { get; set; } = "https://github.com/zPoche/IP-Changer/releases";
}
