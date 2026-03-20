using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.Services;

public sealed class ProfilePersistenceService : IProfilePersistenceService
{
    private readonly ILoggingService _log;
    public static JsonSerializerOptions CreateJsonOptions() => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public ProfilePersistenceService(ILoggingService log)
    {
        _log = log;
    }

    public ProfilesDocument Load()
    {
        try
        {
            var path = AppPaths.ProfilesPath;
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(AppPaths.AppDataFolder);
                return new ProfilesDocument();
            }

            var json = File.ReadAllText(path);
            var doc = JsonSerializer.Deserialize<ProfilesDocument>(json, JsonOptions);
            return doc ?? new ProfilesDocument();
        }
        catch (Exception ex)
        {
            _log.Error("Profile laden fehlgeschlagen.", ex);
            return new ProfilesDocument();
        }
    }

    public void Save(ProfilesDocument document)
    {
        Directory.CreateDirectory(AppPaths.AppDataFolder);
        var path = AppPaths.ProfilesPath;
        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(path, json);
        _log.Info("Profile gespeichert.");
    }
}
