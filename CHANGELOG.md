# Changelog

Alle wichtigen Änderungen werden hier dokumentiert.

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
