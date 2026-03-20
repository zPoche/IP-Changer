using System.Net;
using System.Net.Sockets;

namespace ProfileIpSwitcher.Helpers;

public static class IPv4Validation
{
    public static bool IsValidIpv4(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        return IPAddress.TryParse(text.Trim(), out var ip) && ip.AddressFamily == AddressFamily.InterNetwork;
    }
}
