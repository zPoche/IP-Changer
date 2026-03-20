namespace ProfileIpSwitcher.Models;

/// <summary>Root-Objekt für profiles.json (Versionsfeld für Migrationen).</summary>
public class ProfilesDocument
{
    public int SchemaVersion { get; set; } = 1;

    public List<NetworkProfile> Profiles { get; set; } = new();
}
