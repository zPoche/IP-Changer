using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel(AppSettings settings)
    {
        StartWithWindows = settings.StartWithWindows;
        MinimizeToTrayInsteadOfClose = settings.MinimizeToTrayInsteadOfClose;
        CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup;
        SkipDoubleClickApplyConfirmation = settings.SkipDoubleClickApplyConfirmation;
        IncludeRouteTableInDiagnostics = settings.IncludeRouteTableInDiagnostics;
        AutoCopyDiagnosticsToClipboard = settings.AutoCopyDiagnosticsToClipboard;
        PingCount = settings.PingCount;
        PingTimeoutMs = settings.PingTimeoutMs;
        PortScanParallelism = settings.PortScanParallelism;
        UpdateCheckUrl = settings.UpdateCheckUrl;
        GitHubReleasesUrl = settings.GitHubReleasesUrl;
    }

    public bool StartWithWindows { get; set; }
    public bool MinimizeToTrayInsteadOfClose { get; set; }
    public bool CheckForUpdatesOnStartup { get; set; }
    public bool SkipDoubleClickApplyConfirmation { get; set; }
    public bool IncludeRouteTableInDiagnostics { get; set; }
    public bool AutoCopyDiagnosticsToClipboard { get; set; }
    public int PingCount { get; set; }
    public int PingTimeoutMs { get; set; }
    public int PortScanParallelism { get; set; }
    public string UpdateCheckUrl { get; set; } = string.Empty;
    public string GitHubReleasesUrl { get; set; } = string.Empty;

    public AppSettings ToModel() => new()
    {
        StartWithWindows = StartWithWindows,
        MinimizeToTrayInsteadOfClose = MinimizeToTrayInsteadOfClose,
        CheckForUpdatesOnStartup = CheckForUpdatesOnStartup,
        SkipDoubleClickApplyConfirmation = SkipDoubleClickApplyConfirmation,
        IncludeRouteTableInDiagnostics = IncludeRouteTableInDiagnostics,
        AutoCopyDiagnosticsToClipboard = AutoCopyDiagnosticsToClipboard,
        PingCount = Math.Clamp(PingCount, 1, 20),
        PingTimeoutMs = Math.Clamp(PingTimeoutMs, 500, 15000),
        PortScanParallelism = Math.Clamp(PortScanParallelism, 1, 200),
        UpdateCheckUrl = UpdateCheckUrl.Trim(),
        GitHubReleasesUrl = GitHubReleasesUrl.Trim()
    };
}
