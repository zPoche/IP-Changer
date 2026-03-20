using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public interface ISettingsService
{
    AppSettings Load();

    void Save(AppSettings settings);
}
