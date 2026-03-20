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

namespace ProfileIpSwitcher;

public partial class App : System.Windows.Application
{
}
