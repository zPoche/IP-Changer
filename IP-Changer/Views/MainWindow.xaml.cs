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
    private readonly ILoggingService _log = new LoggingService();
    private bool _shutdownRequested;
    private bool _initialized;
    private bool _startupVisibilityGuardActive = true;
    private bool _forcingStartupState;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += MainWindow_SourceInitialized;
        Loaded += MainWindow_Loaded;
        ContentRendered += MainWindow_ContentRendered;
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;

        // Kaltstart soll immer sichtbar sein; niemals minimiert starten.
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
        Visibility = Visibility.Visible;
        _log.Info("MainWindow: Konstruktor abgeschlossen.");
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        _log.Info("MainWindow.SourceInitialized erreicht.");
        EnsureWindowVisible("SourceInitialized", force: true);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        _initialized = true;
        _log.Info("MainWindow.Loaded: Initialisierung gestartet.");
        EnsureWindowVisible("Loaded-Begin", force: true);

        try
        {
            var settingsService = new SettingsService(_log);
            var persistence = new ProfilePersistenceService(_log);
            var adapters = new NetworkAdapterService(_log);
            var netCfg = new NetworkConfigurationService(_log);
            var updates = new UpdateCheckService(_log, settingsService);
            var dialogs = new DialogService(this);

            _viewModel = new MainViewModel(_log, settingsService, persistence, adapters, netCfg, updates, dialogs);
            DataContext = _viewModel;
            _viewModel.ProfilesChanged += (_, _) => RebuildTrayMenu();
            _viewModel.ToolTargetsChanged += (_, _) => RebuildTrayMenu();

            InitializeTrayIcon();
            RebuildTrayMenu();
            _viewModel.OnStartupUpdateCheck();
            _log.Info("MainWindow.Loaded: Initialisierung abgeschlossen.");
        }
        catch (Exception ex)
        {
            _log.Error("MainWindow.Loaded: Initialisierung fehlgeschlagen.", ex);
            System.Windows.MessageBox.Show(
                this,
                "Beim Start ist ein Fehler aufgetreten.\n" +
                "Die Anwendung bleibt sichtbar, damit der Fehler erkannt werden kann.\n\n" +
                ex.Message,
                "IP-Changer – Startfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            EnsureWindowVisible("Loaded-Ende", force: true);
        }
    }

    private void MainWindow_ContentRendered(object? sender, EventArgs e)
    {
        EnsureWindowVisible("ContentRendered", force: true);
        _startupVisibilityGuardActive = false;
        _log.Info("MainWindow.ContentRendered: Startup-Guard deaktiviert.");
    }

    private void InitializeTrayIcon()
    {
        _log.Info("Tray: Initialisierung gestartet.");
        _notifyIcon = new NotifyIcon { Text = "IP-Changer", Visible = false };

        try
        {
            if (!string.IsNullOrEmpty(Environment.ProcessPath))
                _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
        }
        catch (Exception ex)
        {
            _log.Warn("Tray: Icon konnte nicht geladen werden: " + ex.Message);
        }

        _notifyIcon.DoubleClick += (_, _) => ShowFromTray();
        _log.Info("Tray: Initialisierung abgeschlossen.");
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

        var wolTargets = _viewModel.WakeOnLanTargets.ToList();
        if (wolTargets.Count > 0)
        {
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Werkzeuge");
            foreach (var t in wolTargets)
            {
                var target = t;
                menu.Items.Add($"⚡ WOL {target.Name}", null, async (_, _) =>
                {
                    try
                    {
                        var sent = await _viewModel.SendWakeOnLanFromTrayAsync(target);
                        if (sent)
                            _notifyIcon!.ShowBalloonTip(3500, "IP-Changer", $"WOL gesendet: {target.Name}", ToolTipIcon.Info);
                    }
                    catch (Exception ex)
                    {
                        _notifyIcon!.ShowBalloonTip(6000, "Fehler", ex.Message, ToolTipIcon.Error);
                    }
                });
            }
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
        _log.Info("Tray: Fenster über Tray geöffnet.");
        EnsureWindowVisible("ShowFromTray", force: true);
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_forcingStartupState) return;

        if (_startupVisibilityGuardActive && WindowState == WindowState.Minimized)
        {
            _log.Warn("StateChanged: Minimized während Startup erkannt – Fenster bleibt sichtbar.");
            EnsureWindowVisible("StateChanged-Startup-Minimized", force: true);
            return;
        }

        if (_viewModel?.Settings.MinimizeToTrayInsteadOfClose != true) return;
        if (WindowState != WindowState.Minimized) return;

        Hide();
        if (_notifyIcon != null)
            _notifyIcon.Visible = true;
        _log.Info("StateChanged: Fenster wurde in den Tray minimiert.");
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_shutdownRequested)
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            _log.Info("Closing: reguläres Beenden.");
            return;
        }

        if (_viewModel?.Settings.MinimizeToTrayInsteadOfClose == true)
        {
            e.Cancel = true;
            Hide();
            if (_notifyIcon != null)
                _notifyIcon.Visible = true;
            _log.Info("Closing: Schließen wurde in Tray-Minimierung umgelenkt.");
        }
        else
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            _log.Info("Closing: Beenden ohne Tray-Minimierung.");
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _shutdownRequested = true;
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        _log.Info("Exit_Click: Benutzer hat Beenden gewählt.");
        System.Windows.Application.Current.Shutdown();
    }

    private void EnsureWindowVisible(string source, bool force = false)
    {
        if (!force && !_startupVisibilityGuardActive) return;
        if (_forcingStartupState) return;

        try
        {
            _forcingStartupState = true;

            if (Visibility != Visibility.Visible)
                Visibility = Visibility.Visible;

            if (!IsVisible)
                Show();

            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            if (!ShowInTaskbar)
                ShowInTaskbar = true;

            Activate();
            if (_notifyIcon != null)
                _notifyIcon.Visible = false;

            _log.Info(
                $"VisibilityGuard({source}): IsVisible={IsVisible}, Visibility={Visibility}, " +
                $"WindowState={WindowState}, ShowInTaskbar={ShowInTaskbar}");
        }
        catch (Exception ex)
        {
            _log.Error($"VisibilityGuard({source}) fehlgeschlagen.", ex);
        }
        finally
        {
            _forcingStartupState = false;
        }
    }
}
