namespace ProfileIpSwitcher.Models;

public sealed class UndoableAppliedProfile
{
    public NetworkProfile? Profile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
