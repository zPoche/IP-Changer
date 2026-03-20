using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProfileIpSwitcher.Models;

/// <summary>Ein gespeichertes Netzwerk-/IP-Profil.</summary>
public class NetworkProfile : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string? _description;
    private string _adapterInterfaceId = string.Empty;
    private IpAddressMode _mode = IpAddressMode.Dhcp;
    private string? _ipv4;
    private string? _subnetMask;
    private string? _gateway;
    private bool _isFavorite;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string? Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>Stabiler Schlüssel: <see cref="System.Net.NetworkInformation.NetworkInterface.Id"/>.</summary>
    public string AdapterInterfaceId
    {
        get => _adapterInterfaceId;
        set => SetField(ref _adapterInterfaceId, value);
    }

    public IpAddressMode Mode
    {
        get => _mode;
        set => SetField(ref _mode, value);
    }

    public string? Ipv4
    {
        get => _ipv4;
        set => SetField(ref _ipv4, value);
    }

    public string? SubnetMask
    {
        get => _subnetMask;
        set => SetField(ref _subnetMask, value);
    }

    public string? Gateway
    {
        get => _gateway;
        set => SetField(ref _gateway, value);
    }

    public List<DnsServerEntry> DnsServers { get; set; } = new();

    /// <summary>TODO: WINS-Server bei Bedarf ergänzen.</summary>
    public List<string> WinsServers { get; set; } = new();

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetField(ref _isFavorite, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public NetworkProfile Clone()
    {
        return new NetworkProfile
        {
            Id = Id,
            Name = Name,
            Description = Description,
            AdapterInterfaceId = AdapterInterfaceId,
            Mode = Mode,
            Ipv4 = Ipv4,
            SubnetMask = SubnetMask,
            Gateway = Gateway,
            DnsServers = DnsServers.Select(d => new DnsServerEntry { Address = d.Address }).ToList(),
            WinsServers = WinsServers.ToList(),
            IsFavorite = IsFavorite
        };
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
