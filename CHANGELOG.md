# Changelog

Alle wichtigen Änderungen werden hier dokumentiert.

## [1.1.3] – 2026-03-23

### Geändert

- Ping-Rückmeldung ist jetzt im Hauptbereich sofort sichtbar (prominenter Status-Banner mit ms bzw. Fehlerstatus), auch bei Gateway-/DNS-Ping.
- Kategorien-Umschaltung (`Übersicht` / `Werkzeuge`) wurde deutlich hervorgehoben und wirkt jetzt stärker wie eine echte Bereichsnavigation.
- Akzentfarbe wurde sichtbar aufgehellt, um auf dem Dark-Theme mehr Kontrast und Modernität zu erzeugen.

## [1.1.2] – 2026-03-23

### Behoben

- DNS-Einzelaktionen im Live-Bereich funktionieren wieder zuverlässig (per-item Ping/Kopieren auf synchronisierter DNS-Liste).
- Portscan-Export ist wieder nutzbar: offene Ports werden korrekt erfasst und der Export-Button dynamisch aktiviert.
- Tray-Menü wird bei Änderungen der Wake-on-LAN-Ziele sofort aktualisiert (ohne Neustart).
- Rollback verwendet einen echten Snapshot des Adapterzustands vor der Profilanwendung.

### Geändert

- Tool-Einstellungen wirken jetzt zur Laufzeit:
  - `PingCount` und `PingTimeoutMs` steuern den Ping-Lauf direkt.
  - `PortScanParallelism` steuert den parallelen Portscan.
- Letzte Tool-Eingaben (Ping/Portscan/WoL) werden konsistent gespeichert und erneut geladen.
- UI-Polish für bessere Lesbarkeit und modernere, einheitliche Darstellung:
  - konsistente Action-Button-Stile in Karten und Werkzeugen
  - klarere visuelle Hierarchie und Abstände
  - homogenere Card-Optik im Dark-Theme

## [1.1.0] – 2026-03-23

### Neu

- Neuer Tab **Werkzeuge** mit drei Netzwerktools:
  - **IP-Ping** (IPv4-Test mit Laufzeit/Statusanzeige)
  - **Portscanner (TCP)** mit Portlisten und Portbereichen
  - **Wake-on-LAN** (Magic Packet via Broadcast + UDP-Port)

### Geändert

- Hauptbereich auf **Tab-Navigation** umgestellt: `Übersicht` (Adapter/Profil/Anwenden) und `Werkzeuge` (Diagnose-Tools).
- Ping-Funktion aus dem Live-Adapter-Bereich in den Werkzeug-Tab verschoben.

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
