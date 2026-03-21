using System.Windows;
using System.Windows.Forms;
using MahApps.Metro.Controls;
using ProfileIpSwitcher.Services;
using ProfileIpSwitcher.ViewModels;
using Application = System.Windows.Application;

namespace ProfileIpSwitcher.Views;

public partial class MainWindow
{
    private NotifyIcon? _notifyIcon;
    private MainViewModel? _viewModel;
    private bool _shutdownRequested;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var log = new LoggingService();
        var settingsService = new SettingsService(log);
        var persistence = new ProfilePersistenceService(log);
        var adapters = new NetworkAdapterService(log);
        var netCfg = new NetworkConfigurationService(log);
        var updates = new UpdateCheckService(log, settingsService);
        var dialogs = new DialogService(this);

        _viewModel = new MainViewModel(log, settingsService, persistence, adapters, netCfg, updates, dialogs);
        DataContext = _viewModel;
        _viewModel.ProfilesChanged += (_, _) => RebuildTrayMenu();

        InitializeTrayIcon();
        RebuildTrayMenu();
        _viewModel.OnStartupUpdateCheck();
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "IP-Changer",
            Visible = false
        };

        try
        {
            if (!string.IsNullOrEmpty(Environment.ProcessPath))
                _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
        }
        catch
        {
            /* Icon optional */
        }

        _notifyIcon.DoubleClick += (_, _) => ShowFromTray();
    }

    private void RebuildTrayMenu()
    {
        if (_notifyIcon == null || _viewModel == null) return;

        var menu = new ContextMenuStrip();
        menu.Items.Add("IP-Changer öffnen", null, (_, _) => ShowFromTray());
        menu.Items.Add(new ToolStripSeparator());

        foreach (var p in _viewModel.GetFavoriteProfiles())
        {
            var profile = p;
            menu.Items.Add($"★ {profile.Name}", null, async (_, _) =>
            {
                try
                {
                    var applied = await _viewModel!.ApplyProfileFromTrayAsync(profile);
                    if (applied)
                        _notifyIcon!.ShowBalloonTip(4000, "IP-Changer", "Profil angewendet.", ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    _notifyIcon!.ShowBalloonTip(6000, "Fehler", ex.Message, ToolTipIcon.Error);
                }
            });
        }

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Beenden", null, (_, _) =>
        {
            _shutdownRequested = true;
            System.Windows.Application.Current.Shutdown();
        });

        var previous = _notifyIcon.ContextMenuStrip;
        _notifyIcon.ContextMenuStrip = menu;
        previous?.Dispose();
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        if (_notifyIcon != null)
            _notifyIcon.Visible = false;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_viewModel?.Settings.MinimizeToTrayInsteadOfClose != true) return;
        if (WindowState != WindowState.Minimized) return;
        Hide();
        if (_notifyIcon != null)
            _notifyIcon.Visible = true;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_shutdownRequested)
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            return;
        }

        if (_viewModel?.Settings.MinimizeToTrayInsteadOfClose == true)
        {
            e.Cancel = true;
            Hide();
            if (_notifyIcon != null)
                _notifyIcon.Visible = true;
        }
        else
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _shutdownRequested = true;
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        System.Windows.Application.Current.Shutdown();
    }
}
