/*
 * === ProfileIpSwitcher – Kurz-README ===
 *
 * Abhängigkeiten (NuGet):
 *   - MahApps.Metro (UI)
 *   - System.Management (WMI für Netzwerkprofil u. a.)
 *
 * Ziel: .NET 8, WPF, Windows 10/11 x64. Als Administrator starten (app.manifest requireAdministrator).
 *
 * Pfade unter %AppData%\ProfileIpSwitcher\:
 *   - profiles.json   – gespeicherte Profile (SchemaVersion + profiles[])
 *   - settings.json  – Einstellungen
 *   - logs\app.log   – Rolling-Log (einfache Größenrotation)
 *
 * Bedienung:
 *   Profile links anlegen/bearbeiten, Adapter wählen, „Profil anwenden“ führt netsh aus.
 *   Doppelklick auf Profil: Anwenden mit optionalem Bestätigungsdialog (Einstellungen).
 *   Infobereich: Favoriten-Profile schnell anwenden (Menü „★ Favorit“ setzt genau ein Favorit).
 */

using System.Text;
using System.Windows;
using System.Windows.Threading;
using ProfileIpSwitcher.Services;
using ProfileIpSwitcher.Views;

namespace ProfileIpSwitcher;

public partial class App : System.Windows.Application
{
    private readonly ILoggingService _startupLog = LoggingService.Shared;
    private bool _showingFatalError;
    private bool _exceptionHandlersRegistered;

    public App()
    {
        // Defensive: stellt sicher, dass App.xaml-Ressourcen (Styles/ResourceDictionaries)
        // immer geladen sind, bevor ein Window per Code erstellt wird.
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        RegisterGlobalExceptionHandlers();
        _startupLog.Info("App.OnStartup: Initialisierung beginnt.");

        try
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow
            {
                WindowState = WindowState.Normal,
                ShowInTaskbar = true
            };

            MainWindow = mainWindow;
            mainWindow.Show();
            mainWindow.Activate();

            _startupLog.Info("App.OnStartup: MainWindow wurde sichtbar angezeigt.");
        }
        catch (Exception ex)
        {
            _startupLog.Error("App.OnStartup: Schwerer Startfehler.", ex);
            var rootMessage = GetInnermostMessage(ex);
            ShowFatalError(
                "Die Anwendung konnte nicht korrekt gestartet werden.\n" +
                "Details stehen in der Logdatei unter %AppData%\\ProfileIpSwitcher\\logs\\app.log.\n\n" +
                rootMessage);
            Shutdown(-1);
        }
    }

    private void RegisterGlobalExceptionHandlers()
    {
        if (_exceptionHandlersRegistered) return;
        _exceptionHandlersRegistered = true;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _startupLog.Error("DispatcherUnhandledException", e.Exception);
        ShowFatalError("Unerwarteter Fehler in der UI.\n\n" + GetInnermostMessage(e.Exception));
        e.Handled = true;
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        _startupLog.Error("AppDomain.UnhandledException", ex ?? new Exception("Nicht-Exception-Fehlerobjekt."));
        ShowFatalError("Unerwarteter Fehler (AppDomain).\n\n" + (ex == null ? "Unbekannter Fehler" : GetInnermostMessage(ex)));
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _startupLog.Error("TaskScheduler.UnobservedTaskException", e.Exception);
        e.SetObserved();
    }

    private void ShowFatalError(string message)
    {
        if (_showingFatalError) return;
        _showingFatalError = true;

        try
        {
            var full = new StringBuilder()
                .AppendLine(message)
                .AppendLine()
                .AppendLine("Bitte Anwendung neu starten.")
                .ToString();

            System.Windows.MessageBox.Show(
                full,
                "IP-Changer – Startfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            // Fehlerdialog darf keinen Folgefehler auslösen.
        }
        finally
        {
            _showingFatalError = false;
        }
    }

    private static string GetInnermostMessage(Exception ex)
    {
        var current = ex;
        while (current.InnerException != null)
            current = current.InnerException;
        return current.Message;
    }
}
