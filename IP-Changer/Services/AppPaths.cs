using System.IO;

namespace ProfileIpSwitcher.Services;

public static class AppPaths
{
    public static string AppDataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProfileIpSwitcher");

    public static string ProfilesPath => Path.Combine(AppDataFolder, "profiles.json");

    public static string SettingsPath => Path.Combine(AppDataFolder, "settings.json");
}
