namespace ProfileIpSwitcher.Models;

/// <summary>Aktuelle Informationen zu einem Netzwerkadapter.</summary>
public class NetworkAdapterInfo
{
    public string InterfaceId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string OperationalStatus { get; set; } = string.Empty;

    public bool IsWireless { get; set; }

    public string Ipv4 { get; set; } = "—";

    public string SubnetMask { get; set; } = "—";

    public string Gateway { get; set; } = "—";

    public string DnsServers { get; set; } = "—";

    public bool DhcpEnabled { get; set; }

    public string NetworkCategory { get; set; } = "Unbekannt";

    public string? WifiSsid { get; set; }

    public string DisplayLine => $"{Name} ({Description})";
}
