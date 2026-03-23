using System.Collections.ObjectModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using ProfileIpSwitcher.Helpers;
using ProfileIpSwitcher.Models;
using ProfileIpSwitcher.Services;
using ProfileIpSwitcher.Views;

namespace ProfileIpSwitcher.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly ILoggingService _log;
    private readonly ISettingsService _settingsService;
    private readonly IProfilePersistenceService _persistence;
    private readonly INetworkAdapterService _adapters;
    private readonly INetworkConfigurationService _netCfg;
    private readonly IUpdateCheckService _updates;
    private readonly IDialogService _dialogs;

    private string _searchText = string.Empty;
    private NetworkProfile? _selectedProfile;
    private NetworkAdapterInfo? _selectedStatusAdapter;
    private bool _isBusy;
    private string _lastOperation = "—";
    private string _currentTime = string.Empty;
    private bool _isElevated = true;
    private string _pingTarget = "8.8.8.8";
    private string _pingResult = "—";
    private bool _isPinging;

    public MainViewModel(
        ILoggingService log,
        ISettingsService settingsService,
        IProfilePersistenceService persistence,
        INetworkAdapterService adapters,
        INetworkConfigurationService netCfg,
        IUpdateCheckService updates,
        IDialogService dialogs)
    {
        _log = log;
        _settingsService = settingsService;
        _persistence = persistence;
        _adapters = adapters;
        _netCfg = netCfg;
        _updates = updates;
        _dialogs = dialogs;

        RefreshAdaptersCommand = new RelayCommand(_ => RefreshAdapters());
        NewProfileCommand = new RelayCommand(_ => NewProfile());
        EditProfileCommand = new RelayCommand(_ => EditProfile(), _ => SelectedProfile != null);
        DuplicateProfileCommand = new RelayCommand(_ => DuplicateProfile(), _ => SelectedProfile != null);
        DeleteProfileCommand = new RelayCommand(_ => DeleteProfile(), _ => SelectedProfile != null);
        ToggleFavoriteCommand = new RelayCommand(_ => ToggleFavorite(), _ => SelectedProfile != null);
        ApplyProfileCommand = new AsyncRelayCommand(_ => ApplySelectedAsync(skipConfirm: true), _ => CanApplyInternal());
        ApplyProfileDoubleClickCommand = new AsyncRelayCommand(_ => ApplySelectedAsync(skipConfirm: false), _ => CanApplyInternal());
        ExportCommand = new RelayCommand(_ => ExportProfiles());
        ImportCommand = new RelayCommand(_ => ImportProfiles());
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        CheckUpdatesCommand = new AsyncRelayCommand(_ => CheckUpdatesAsync(fromStartup: false));
        OpenAboutCommand = new RelayCommand(_ => OpenAbout());
        PingCommand = new AsyncRelayCommand(_ => PingAsync(), _ => CanPing);

        LoadProfiles();
        Settings = _settingsService.Load();
        _isElevated = ElevationHelper.IsProcessElevated();
        if (!_isElevated)
        {
            _lastOperation = "Keine Administratorrechte – Anwenden deaktiviert.";
            _dialogs.ShowWarning(
                "ProfileIpSwitcher sollte als Administrator gestartet werden, um IP-Einstellungen zu ändern.\n" +
                "Die Schaltfläche „Profil anwenden“ ist deaktiviert.",
                "Hinweis");
        }

        RefreshAdapters();

        var clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        clock.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        clock.Start();
        CurrentTime = DateTime.Now.ToString("HH:mm:ss");

        _log.Info("Anwendung gestartet.");
    }

    public AppSettings Settings { get; private set; }

    public ObservableCollection<NetworkProfile> Profiles { get; } = new();

    public ObservableCollection<NetworkProfile> FilteredProfiles { get; } = new();

    public ObservableCollection<NetworkAdapterInfo> Adapters { get; } = new();

    public event EventHandler? ProfilesChanged;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter();
        }
    }

    public NetworkProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value))
            {
                Raise(nameof(CanApply));
                ((RelayCommand)EditProfileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DuplicateProfileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteProfileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ToggleFavoriteCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ApplyProfileCommand).NotifyCanExecuteChanged();
                ((AsyncRelayCommand)ApplyProfileDoubleClickCommand).NotifyCanExecuteChanged();
                SyncStatusAdapterWithProfile();
                RaiseProfilePreviewProperties();
            }
        }
    }

    public NetworkAdapterInfo? SelectedStatusAdapter
    {
        get => _selectedStatusAdapter;
        set
        {
            if (SetProperty(ref _selectedStatusAdapter, value))
                RaiseLiveAdapterProperties();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                Raise(nameof(CanApply));
                ((AsyncRelayCommand)ApplyProfileCommand).NotifyCanExecuteChanged();
                ((AsyncRelayCommand)ApplyProfileDoubleClickCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string LastOperation
    {
        get => _lastOperation;
        set => SetProperty(ref _lastOperation, value);
    }

    public string CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }

    public bool IsElevated => _isElevated;

    public bool CanApply => CanApplyInternal();
    public bool CanPing => !IsPinging && !string.IsNullOrWhiteSpace(PingTarget);

    public bool HasSelectedProfile => SelectedProfile != null;

    public string ProfilePreviewName => SelectedProfile?.Name ?? string.Empty;

    public string ProfilePreviewDescription =>
        string.IsNullOrWhiteSpace(SelectedProfile?.Description) ? "—" : SelectedProfile!.Description!;

    public string ProfilePreviewAdapter => FormatProfileAdapterLine(SelectedProfile);

    public string ProfilePreviewMode => SelectedProfile == null ? string.Empty : FormatMode(SelectedProfile.Mode);

    public string ProfilePreviewIpv4 => SelectedProfile == null ? string.Empty : (SelectedProfile.Ipv4 ?? "—");

    public string ProfilePreviewSubnet => SelectedProfile == null ? string.Empty : (SelectedProfile.SubnetMask ?? "—");

    public string ProfilePreviewGateway => SelectedProfile == null ? string.Empty : (SelectedProfile.Gateway ?? "—");

    public string ProfilePreviewDns
    {
        get
        {
            if (SelectedProfile == null) return string.Empty;
            var dns = string.Join(", ", SelectedProfile.DnsServers.Select(d => d.Address?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a)));
            return string.IsNullOrEmpty(dns) ? "—" : dns;
        }
    }

    public string ProfilePreviewFavorite =>
        SelectedProfile == null ? string.Empty : (SelectedProfile.IsFavorite ? "Ja" : "Nein");

    public string LiveOperationalStatus => SelectedStatusAdapter?.OperationalStatus ?? "—";

    public string LiveIpv4 => SelectedStatusAdapter?.Ipv4 ?? "—";

    public string LiveSubnetMask => SelectedStatusAdapter?.SubnetMask ?? "—";

    public string LiveGateway => SelectedStatusAdapter?.Gateway ?? "—";

    public string LiveDns
    {
        get
        {
            var a = SelectedStatusAdapter;
            if (a == null) return "—";
            if (string.IsNullOrWhiteSpace(a.DnsServers) || a.DnsServers == "—") return "—";
            return a.DnsServers;
        }
    }

    public string LiveDhcp => SelectedStatusAdapter == null ? "—" : (SelectedStatusAdapter.DhcpEnabled ? "Ja" : "Nein");

    public string LiveNetworkCategory => SelectedStatusAdapter?.NetworkCategory ?? "—";

    public string LiveWifiSsid
    {
        get
        {
            if (SelectedStatusAdapter == null) return "—";
            if (!SelectedStatusAdapter.IsWireless) return "—";
            var s = SelectedStatusAdapter.WifiSsid;
            return string.IsNullOrWhiteSpace(s) ? "Nicht verbunden" : s;
        }
    }

    public NetworkAdapterInfo? CurrentAdapterStatus => SelectedStatusAdapter;

    public string PingTarget
    {
        get => _pingTarget;
        set
        {
            if (SetProperty(ref _pingTarget, value))
            {
                Raise(nameof(CanPing));
                ((AsyncRelayCommand)PingCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public string PingResult
    {
        get => _pingResult;
        set => SetProperty(ref _pingResult, value);
    }

    public bool IsPinging
    {
        get => _isPinging;
        set
        {
            if (SetProperty(ref _isPinging, value))
            {
                Raise(nameof(CanPing));
                ((AsyncRelayCommand)PingCommand).NotifyCanExecuteChanged();
            }
        }
    }

    public ICommand RefreshAdaptersCommand { get; }
    public ICommand NewProfileCommand { get; }
    public ICommand EditProfileCommand { get; }
    public ICommand DuplicateProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ApplyProfileCommand { get; }
    public ICommand ApplyProfileDoubleClickCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand CheckUpdatesCommand { get; }
    public ICommand OpenAboutCommand { get; }
    public ICommand PingCommand { get; }

    public IReadOnlyList<NetworkProfile> GetFavoriteProfiles() =>
        Profiles.Where(p => p.IsFavorite).ToList();

    public async Task<bool> ApplyProfileFromTrayAsync(NetworkProfile profile)
    {
        SelectedProfile = profile;
        return await ApplySelectedAsync(skipConfirm: true);
    }

    private bool CanApplyInternal() =>
        _isElevated && !IsBusy && SelectedProfile != null;

    private void LoadProfiles()
    {
        var doc = _persistence.Load();
        Profiles.Clear();
        foreach (var p in doc.Profiles.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            Profiles.Add(p);
        ApplyFilter();
        RefreshProfileAdapterSubtitles();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SaveProfiles()
    {
        var doc = new ProfilesDocument { SchemaVersion = 1, Profiles = Profiles.ToList() };
        _persistence.Save(doc);
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyFilter()
    {
        var q = SearchText.Trim();
        FilteredProfiles.Clear();
        foreach (var p in Profiles)
        {
            if (string.IsNullOrEmpty(q))
            {
                FilteredProfiles.Add(p);
                continue;
            }

            var adapter = _adapters.FindByInterfaceId(p.AdapterInterfaceId);
            var adapterText = adapter?.DisplayLine ?? p.AdapterInterfaceId;
            if (p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                adapterText.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
                FilteredProfiles.Add(p);
        }
    }

    private void RefreshAdapters()
    {
        var list = _adapters.RefreshAdapters();
        Adapters.Clear();
        foreach (var a in list.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            Adapters.Add(a);

        SelectedStatusAdapter ??= Adapters.FirstOrDefault(a => a.OperationalStatus == "Up") ?? Adapters.FirstOrDefault();
        ApplyFilter();
        RefreshProfileAdapterSubtitles();
        RaiseProfilePreviewProperties();
        RaiseLiveAdapterProperties();
    }

    private void SyncStatusAdapterWithProfile()
    {
        if (SelectedProfile == null) return;
        var match = Adapters.FirstOrDefault(a =>
            string.Equals(a.InterfaceId, SelectedProfile.AdapterInterfaceId, StringComparison.OrdinalIgnoreCase));
        if (match != null)
            SelectedStatusAdapter = match;
    }

    private void NewProfile()
    {
        var vm = new ProfileEditViewModel(new NetworkProfile(), Adapters.ToList());
        if (!ProfileEditWindow.ShowDialog(vm, System.Windows.Application.Current.MainWindow, () => SelectedStatusAdapter))
            return;
        if (!vm.TryBuildProfile(out var created, out var err))
        {
            _dialogs.ShowWarning(err, "Profil");
            return;
        }

        SearchText = "";
        Profiles.Add(created);
        SaveProfiles();
        ApplyFilter();
        SelectedProfile = created;
        RefreshProfileAdapterSubtitles();
    }

    private void EditProfile()
    {
        if (SelectedProfile == null) return;
        var vm = new ProfileEditViewModel(SelectedProfile.Clone(), Adapters.ToList());
        if (!ProfileEditWindow.ShowDialog(vm, System.Windows.Application.Current.MainWindow, () => SelectedStatusAdapter))
            return;
        if (!vm.TryBuildProfile(out var updated, out var err))
        {
            _dialogs.ShowWarning(err, "Profil");
            return;
        }

        updated.IsFavorite = SelectedProfile.IsFavorite;
        var idx = Profiles.IndexOf(SelectedProfile);
        if (idx >= 0)
        {
            Profiles[idx] = updated;
            SaveProfiles();
            ApplyFilter();
            SelectedProfile = updated;
            RefreshProfileAdapterSubtitles();
            RaiseProfilePreviewProperties();
        }
    }

    private void DuplicateProfile()
    {
        if (SelectedProfile == null) return;
        var copy = SelectedProfile.Clone();
        copy.Id = Guid.NewGuid();
        copy.Name = copy.Name + " (Kopie)";
        copy.IsFavorite = false;
        Profiles.Add(copy);
        SaveProfiles();
        ApplyFilter();
        SelectedProfile = copy;
        RefreshProfileAdapterSubtitles();
    }

    private void DeleteProfile()
    {
        if (SelectedProfile == null) return;
        if (!_dialogs.AskYesNo($"Profil „{SelectedProfile.Name}“ löschen?", "Löschen"))
            return;
        Profiles.Remove(SelectedProfile);
        SaveProfiles();
        ApplyFilter();
        SelectedProfile = FilteredProfiles.FirstOrDefault();
    }

    private void ToggleFavorite()
    {
        if (SelectedProfile == null) return;
        var newVal = !SelectedProfile.IsFavorite;
        if (newVal)
        {
            foreach (var p in Profiles)
                p.IsFavorite = false;
        }

        SelectedProfile.IsFavorite = newVal;
        SaveProfiles();
        RaiseProfilePreviewProperties();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<bool> ApplySelectedAsync(bool skipConfirm)
    {
        if (SelectedProfile == null || !_isElevated) return false;

        try
        {
            if (!skipConfirm)
            {
                var (apply, dontAsk) = _dialogs.ConfirmApplyProfile(SelectedProfile.Name,
                    Settings.SkipDoubleClickApplyConfirmation);
                if (!apply) return false;
                if (dontAsk)
                {
                    Settings.SkipDoubleClickApplyConfirmation = true;
                    _settingsService.Save(Settings);
                    Raise(nameof(Settings));
                }
            }

            var netshName = _adapters.GetNetshInterfaceName(SelectedProfile.AdapterInterfaceId);
            if (string.IsNullOrEmpty(netshName))
            {
                _dialogs.ShowWarning(
                    "Adapter für dieses Profil wurde nicht gefunden. Bitte Adapter aktualisieren oder Profil bearbeiten.",
                    "Fehler");
                LastOperation = "Fehler: Adapter nicht gefunden.";
                return false;
            }

            IsBusy = true;
            try
            {
                var result = await _netCfg.ApplyProfileAsync(SelectedProfile, netshName);
                if (result.Success)
                {
                    LastOperation = "OK: " + result.Message;
                    _log.Info($"Profil angewendet: {SelectedProfile.Name}");
                    _dialogs.ShowInformation(result.Message, "Erfolg");
                    await Task.Delay(1300, CancellationToken.None);
                    RefreshAdapters();
                    return true;
                }

                LastOperation = "Fehler: " + result.Message;
                _log.Error("netsh: " + result.Message + " " + result.StandardError);
                _dialogs.ShowError(result.Message + Environment.NewLine + result.StandardError, "Fehler");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            IsBusy = false;
            _log.Error("Profil anwenden", ex);
            LastOperation = "Unerwarteter Fehler beim Anwenden.";
            _dialogs.ShowError("Profil konnte nicht angewendet werden:\n" + ex.Message, "Fehler");
            return false;
        }
    }

    private void ExportProfiles()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON (*.json)|*.json",
            FileName = "ProfileIpSwitcher-export.json"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var doc = new ProfilesDocument { SchemaVersion = 1, Profiles = Profiles.ToList() };
            var json = JsonSerializer.Serialize(doc, ProfilePersistenceService.CreateJsonOptions());
            File.WriteAllText(dlg.FileName, json);
            LastOperation = "Export abgeschlossen.";
        }
        catch (Exception ex)
        {
            _log.Error("Export", ex);
            _dialogs.ShowError("Export fehlgeschlagen: " + ex.Message, "Fehler");
        }
    }

    private void ImportProfiles()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON (*.json)|*.json" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var json = File.ReadAllText(dlg.FileName);
            var doc = JsonSerializer.Deserialize<ProfilesDocument>(json, ProfilePersistenceService.CreateJsonOptions());
            if (doc?.Profiles == null || doc.Profiles.Count == 0)
            {
                _dialogs.ShowWarning("Keine gültigen Profile in der Datei.", "Import");
                return;
            }

            foreach (var p in doc.Profiles)
            {
                p.Id = Guid.NewGuid();
                p.Name = string.IsNullOrWhiteSpace(p.Name) ? "Importiert" : p.Name.Trim() + " (importiert)";
                p.IsFavorite = false;
                Profiles.Add(p);
            }

            SaveProfiles();
            ApplyFilter();
            RefreshProfileAdapterSubtitles();
            LastOperation = $"Import: {doc.Profiles.Count} Profil(e).";
        }
        catch (Exception ex)
        {
            _log.Error("Import", ex);
            _dialogs.ShowError("Import fehlgeschlagen: " + ex.Message, "Fehler");
        }
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(Settings);
        if (!SettingsWindow.ShowDialog(vm, System.Windows.Application.Current.MainWindow))
            return;
        var updated = vm.ToModel();
        _settingsService.Save(updated);
        Settings = updated;
        Raise(nameof(Settings));
        LastOperation = "Einstellungen gespeichert.";
    }

    private void OpenAbout()
    {
        var owner = System.Windows.Application.Current.MainWindow;
        AboutWindow.ShowDialog(owner);
    }

    private async Task CheckUpdatesAsync(bool fromStartup)
    {
        var r = await _updates.CheckAsync(Settings.UpdateCheckUrl);

        if (!r.Success)
        {
            if (!fromStartup)
                _dialogs.ShowWarning(r.Message, "Update-Prüfung");
            else
                _log.Warn("Update (Start): " + r.Message);

            LastOperation = "Update-Check fehlgeschlagen.";
            return;
        }

        if (r.UpdateAvailable)
        {
            LastOperation = $"Update verfügbar: {r.LatestVersion}";
            if (_dialogs.AskYesNo($"{r.Message}\n\nRelease-Seite im Browser öffnen?", "Update verfügbar"))
                UpdateCheckService.OpenReleasesPage(r.ReleasesPageUrl);
            return;
        }

        LastOperation = "Kein Update verfügbar.";
        if (!fromStartup)
        {
            _dialogs.ShowInformation($"{r.Message}\n\nInstalliert: {r.CurrentVersion}", "Update-Prüfung");
        }
    }

    private async Task PingAsync()
    {
        var target = PingTarget.Trim();
        if (!IPv4Validation.IsValidIpv4(target))
        {
            PingResult = "Ungültige IPv4-Adresse.";
            LastOperation = "Ping fehlgeschlagen: Ungültige IPv4-Adresse.";
            return;
        }

        IsPinging = true;
        PingResult = $"Ping läuft zu {target} …";
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(target, 2000);
            if (reply.Status == IPStatus.Success)
            {
                var replyAddress = reply.Address?.ToString() ?? target;
                PingResult = $"Antwort von {replyAddress}: Zeit={reply.RoundtripTime} ms";
                LastOperation = $"Ping erfolgreich: {replyAddress} ({reply.RoundtripTime} ms).";
                _log.Info($"Ping erfolgreich: {replyAddress} in {reply.RoundtripTime} ms.");
                return;
            }

            PingResult = $"Keine Antwort von {target} ({reply.Status}).";
            LastOperation = $"Ping fehlgeschlagen: {reply.Status}.";
            _log.Warn($"Ping fehlgeschlagen ({target}): {reply.Status}.");
        }
        catch (PingException ex)
        {
            PingResult = $"Ping-Fehler: {ex.InnerException?.Message ?? ex.Message}";
            LastOperation = "Ping fehlgeschlagen.";
            _log.Warn("Ping-Fehler: " + ex.Message);
        }
        catch (Exception ex)
        {
            PingResult = $"Unerwarteter Fehler: {ex.Message}";
            LastOperation = "Ping fehlgeschlagen.";
            _log.Error("Ping", ex);
        }
        finally
        {
            IsPinging = false;
        }
    }

    public void OnStartupUpdateCheck()
    {
        if (!Settings.CheckForUpdatesOnStartup) return;
        _ = CheckUpdatesAsync(fromStartup: true);
    }

    private void RaiseProfilePreviewProperties()
    {
        Raise(nameof(HasSelectedProfile));
        Raise(nameof(ProfilePreviewName));
        Raise(nameof(ProfilePreviewDescription));
        Raise(nameof(ProfilePreviewAdapter));
        Raise(nameof(ProfilePreviewMode));
        Raise(nameof(ProfilePreviewIpv4));
        Raise(nameof(ProfilePreviewSubnet));
        Raise(nameof(ProfilePreviewGateway));
        Raise(nameof(ProfilePreviewDns));
        Raise(nameof(ProfilePreviewFavorite));
    }

    private void RaiseLiveAdapterProperties()
    {
        Raise(nameof(CurrentAdapterStatus));
        Raise(nameof(LiveOperationalStatus));
        Raise(nameof(LiveIpv4));
        Raise(nameof(LiveSubnetMask));
        Raise(nameof(LiveGateway));
        Raise(nameof(LiveDns));
        Raise(nameof(LiveDhcp));
        Raise(nameof(LiveNetworkCategory));
        Raise(nameof(LiveWifiSsid));
    }

    private void RefreshProfileAdapterSubtitles()
    {
        foreach (var p in Profiles)
        {
            var a = _adapters.FindByInterfaceId(p.AdapterInterfaceId);
            p.AdapterListSubtitle = a == null
                ? "Adapter nicht gefunden"
                : (a.IsWireless ? "WLAN · " : "LAN · ") + a.DisplayLine;
        }
    }

    private string FormatProfileAdapterLine(NetworkProfile? p)
    {
        if (p == null) return string.Empty;
        var a = _adapters.FindByInterfaceId(p.AdapterInterfaceId);
        if (a == null) return "Adapter nicht gefunden";
        return (a.IsWireless ? "WLAN · " : "LAN · ") + a.DisplayLine;
    }

    private static string FormatMode(IpAddressMode m) => m == IpAddressMode.Dhcp ? "DHCP" : "Statisch";
}

