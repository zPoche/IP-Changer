# Changelog

Alle wichtigen Änderungen werden hier dokumentiert.

## [1.1.1] – 2026-03-24

### Behoben

- Netzwerkkategorie (Öffentlich/Privat/Domäne) wurde nie übersetzt – WMI `uint32` matchte nicht gegen `int`-Literale
- Portscan: unbehandelte Task-Exceptions bei fehlgeschlagenen TCP-Verbindungen

### Verbessert

- Ping akzeptiert jetzt auch Hostnamen (DNS-Auflösung wie Portscan)
- Adapter-Refresh läuft asynchron im Threadpool (UI friert nicht mehr ein)
- Korruptionsschutz für `profiles.json` (Backup bei defekter Datei)
- LoggingService als Singleton (eine gemeinsame Instanz)
- MainViewModel implementiert `IDisposable` (Timer-Cleanup)

### Aufgeräumt

- Werkzeuge (Ping, Portscan, WoL) in eigenes `ToolsViewModel` extrahiert
- `UpdateCheckResult` in eigene Datei verschoben
- Unbenutzte `WinsServers`-Eigenschaft und leeres `<ApplicationIcon />`-Tag entfernt

## [1.0.9] – 2026-03-23

### Neu

- Live-Adapter-Bereich um ein **IP-Ping-Feature** erweitert (IPv4-Ziel eingeben, Ping starten, Ergebnis direkt in der Oberfläche).

### Technisch

- `MainViewModel`: neuer `PingCommand` mit asynchroner Ping-Logik (`System.Net.NetworkInformation.Ping`), IPv4-Validierung, Busy-Status und Logging.
- `MainWindow`: neue UI-Zeile für Ping-Eingabe, Start-Button, ProgressRing und Ergebnisanzeige.

## [1.0.3] – 2026-03-20

### Geändert

- Anzeigename **IP-Changer** (Fenstertitel, Infobereich)
- Menü **Extras → Info…** mit About-Dialog (Version, GitHub-Link)

## [1.0.2] – 2026-03-20

### Behoben

- Nach erfolgreichem Profil anwenden: kurze Verzögerung, dann Adapter-Refresh, damit der Live-Bereich (z. B. DHCP) mit WMI wieder zur echten Konfiguration passt

## [1.0.1] – 2026-03-20

### Verbesserungen

- DHCP-Anzeige über WMI (`Win32_NetworkAdapterConfiguration`), mit Fallback
- Update-Check: wiederverwendbarer `HttpClient`, klarere Struktur
- Einstellungen im MainViewModel konsistenter; UI: Profilvorschau & Live-Adapter als Tabellen, Karten-Layout, Busy-Indikator
- Profilliste: lesbarer Adapter-Hinweis (WLAN/LAN) statt roher Interface-ID

## [1.0.0-alpha.1] – 2026-03-12

### Erste öffentliche Alpha

- WPF-App (.NET 8) mit MahApps.Metro: IP-/DNS-Profile pro Netzwerkadapter
- Profilverwaltung (anlegen, bearbeiten, duplizieren, Favorit, Suche)
- Anwenden per `netsh` (Admin-Rechte erforderlich)
- Adapter-Erkennung inkl. WLAN-SSID und Netzwerkprofil (WMI)
- Import/Export der Profile (JSON)
- Infobereich, Einstellungen, Update-Check (GitHub Releases)
- Logs unter `%AppData%\ProfileIpSwitcher\logs\`

**Hinweis:** Alpha – vor Produktiveinsatz bitte testen. Feedback willkommen.
