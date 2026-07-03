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

    public bool IncludeRouteTableInDiagnostics { get; set; } = true;

    public bool AutoCopyDiagnosticsToClipboard { get; set; } = true;

    public int PingCount { get; set; } = 4;

    public int PingTimeoutMs { get; set; } = 2000;

    public int PortScanParallelism { get; set; } = 50;

    public string LastPingTarget { get; set; } = "8.8.8.8";

    public string LastPortScanTarget { get; set; } = "127.0.0.1";

    public string LastPortScanPorts { get; set; } = "22,80,443";

    public string LastWakeOnLanMac { get; set; } = string.Empty;

    public string LastWakeOnLanBroadcast { get; set; } = "255.255.255.255";

    public string LastWakeOnLanPort { get; set; } = "9";

}
