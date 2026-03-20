using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly AppSettings _settings;

    public SettingsViewModel(AppSettings settings)
    {
        _settings = settings;
        StartWithWindows = settings.StartWithWindows;
        MinimizeToTrayInsteadOfClose = settings.MinimizeToTrayInsteadOfClose;
        CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup;
        SkipDoubleClickApplyConfirmation = settings.SkipDoubleClickApplyConfirmation;
        UpdateCheckUrl = settings.UpdateCheckUrl;
        GitHubReleasesUrl = settings.GitHubReleasesUrl;
    }

    public bool StartWithWindows { get; set; }
    public bool MinimizeToTrayInsteadOfClose { get; set; }
    public bool CheckForUpdatesOnStartup { get; set; }
    public bool SkipDoubleClickApplyConfirmation { get; set; }
    public string UpdateCheckUrl { get; set; } = string.Empty;
    public string GitHubReleasesUrl { get; set; } = string.Empty;

    public AppSettings ToModel() => new()
    {
        StartWithWindows = StartWithWindows,
        MinimizeToTrayInsteadOfClose = MinimizeToTrayInsteadOfClose,
        CheckForUpdatesOnStartup = CheckForUpdatesOnStartup,
        SkipDoubleClickApplyConfirmation = SkipDoubleClickApplyConfirmation,
        UpdateCheckUrl = UpdateCheckUrl.Trim(),
        GitHubReleasesUrl = GitHubReleasesUrl.Trim()
    };
}
