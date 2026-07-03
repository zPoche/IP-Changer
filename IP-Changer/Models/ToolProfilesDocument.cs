namespace ProfileIpSwitcher.Models;

public sealed class ToolProfilesDocument
{
    public int SchemaVersion { get; set; } = 1;

    public List<WakeOnLanTarget> WakeOnLanTargets { get; set; } = new();
}
