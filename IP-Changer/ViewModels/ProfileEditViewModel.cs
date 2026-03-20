using System.Collections.ObjectModel;
using ProfileIpSwitcher.Helpers;
using ProfileIpSwitcher.Models;

namespace ProfileIpSwitcher.ViewModels;

/// <summary>ViewModel für Profil bearbeiten / neu anlegen.</summary>
public sealed class ProfileEditViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string? _description;
    private string _adapterInterfaceId = string.Empty;
    private IpAddressMode _mode = IpAddressMode.Dhcp;
    private string? _ipv4;
    private string? _subnetMask;
    private string? _gateway;
    private string _dnsText = string.Empty;
    private bool _ipInvalid;
    private bool _maskInvalid;
    private bool _gwInvalid;

    public ProfileEditViewModel(NetworkProfile source, IReadOnlyList<NetworkAdapterInfo> adapters)
    {
        Adapters = adapters.ToList();
        Id = source.Id;
        Name = source.Name;
        Description = source.Description;
        AdapterInterfaceId = source.AdapterInterfaceId;
        Mode = source.Mode;
        Ipv4 = source.Ipv4;
        SubnetMask = source.SubnetMask;
        Gateway = source.Gateway;
        DnsText = string.Join(Environment.NewLine, source.DnsServers.Select(d => d.Address));
    }

    public Guid Id { get; }

    public List<NetworkAdapterInfo> Adapters { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string AdapterInterfaceId
    {
        get => _adapterInterfaceId;
        set => SetProperty(ref _adapterInterfaceId, value);
    }

    public IpAddressMode Mode
    {
        get => _mode;
        set
        {
            if (SetProperty(ref _mode, value))
                ValidateIpFields();
        }
    }

    public List<ModeOption> ModeOptions { get; } = new()
    {
        new(IpAddressMode.Dhcp, "DHCP"),
        new(IpAddressMode.Static, "Statisch")
    };

    public record ModeOption(IpAddressMode Value, string Text);

    public string? Ipv4
    {
        get => _ipv4;
        set
        {
            if (SetProperty(ref _ipv4, value))
                ValidateIpFields();
        }
    }

    public string? SubnetMask
    {
        get => _subnetMask;
        set
        {
            if (SetProperty(ref _subnetMask, value))
                ValidateIpFields();
        }
    }

    public string? Gateway
    {
        get => _gateway;
        set
        {
            if (SetProperty(ref _gateway, value))
                ValidateIpFields();
        }
    }

    public string DnsText
    {
        get => _dnsText;
        set => SetProperty(ref _dnsText, value);
    }

    public bool IpInvalid
    {
        get => _ipInvalid;
        set => SetProperty(ref _ipInvalid, value);
    }

    public bool MaskInvalid
    {
        get => _maskInvalid;
        set => SetProperty(ref _maskInvalid, value);
    }

    public bool GatewayInvalid
    {
        get => _gwInvalid;
        set => SetProperty(ref _gwInvalid, value);
    }

    public void LoadFromAdapter(NetworkAdapterInfo? a)
    {
        if (a == null) return;
        AdapterInterfaceId = a.InterfaceId;
        Ipv4 = a.Ipv4 is "—" or "" ? null : a.Ipv4;
        SubnetMask = a.SubnetMask is "—" or "" ? null : a.SubnetMask;
        Gateway = a.Gateway is "—" or "" ? null : a.Gateway;
        if (a.DnsServers is not ("—" or ""))
            DnsText = string.Join(Environment.NewLine, a.DnsServers.Split(',', StringSplitOptions.TrimEntries));
        Mode = a.DhcpEnabled ? IpAddressMode.Dhcp : IpAddressMode.Static;
    }

    public bool TryBuildProfile(out NetworkProfile profile, out string error)
    {
        profile = new NetworkProfile { Id = Id };
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            error = "Bitte einen Profilnamen eingeben.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(AdapterInterfaceId))
        {
            error = "Bitte einen Netzwerkadapter auswählen.";
            return false;
        }

        if (Mode == IpAddressMode.Static)
        {
            if (!IPv4Validation.IsValidIpv4(Ipv4))
            {
                error = "Ungültige IPv4-Adresse.";
                return false;
            }

            if (!IPv4Validation.IsValidIpv4(SubnetMask))
            {
                error = "Ungültige Subnetzmaske.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Gateway) && !IPv4Validation.IsValidIpv4(Gateway))
            {
                error = "Ungültiges Gateway.";
                return false;
            }
        }

        profile.Name = Name.Trim();
        profile.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        profile.AdapterInterfaceId = AdapterInterfaceId;
        profile.Mode = Mode;
        profile.Ipv4 = string.IsNullOrWhiteSpace(Ipv4) ? null : Ipv4.Trim();
        profile.SubnetMask = string.IsNullOrWhiteSpace(SubnetMask) ? null : SubnetMask.Trim();
        profile.Gateway = string.IsNullOrWhiteSpace(Gateway) ? null : Gateway.Trim();
        profile.DnsServers = DnsText
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => new DnsServerEntry { Address = s })
            .ToList();

        foreach (var d in profile.DnsServers)
        {
            if (!IPv4Validation.IsValidIpv4(d.Address))
            {
                error = $"Ungültige DNS-Adresse: {d.Address}";
                return false;
            }
        }

        return true;
    }

    private void ValidateIpFields()
    {
        IpInvalid = Mode == IpAddressMode.Static && !string.IsNullOrWhiteSpace(Ipv4) &&
                    !IPv4Validation.IsValidIpv4(Ipv4);
        MaskInvalid = Mode == IpAddressMode.Static && !string.IsNullOrWhiteSpace(SubnetMask) &&
                      !IPv4Validation.IsValidIpv4(SubnetMask);
        GatewayInvalid = Mode == IpAddressMode.Static && !string.IsNullOrWhiteSpace(Gateway) &&
                         !IPv4Validation.IsValidIpv4(Gateway);
    }
}
