namespace ProfileIpSwitcher.Models;

public sealed class WakeOnLanTarget
{
    public string Name { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string BroadcastAddress { get; set; } = "255.255.255.255";
    public int Port { get; set; } = 9;
}
