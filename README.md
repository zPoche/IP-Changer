# ProfileIpSwitcher

Windows-Desktop-App (.NET 8, WPF, MahApps.Metro) zum schnellen Wechsel von IPv4-/DNS-Einstellungen pro Netzwerkadapter (ähnlich wie NetSetMan, reduzierter Funktionsumfang).

## Build & Start

```powershell
cd ProfileIpSwitcher
dotnet build
dotnet run --project ProfileIpSwitcher\ProfileIpSwitcher.csproj
```

Die ausführbare Datei liegt unter `ProfileIpSwitcher\bin\Debug\net8.0-windows\` (bzw. `Release`).  
**Hinweis:** Über `app.manifest` ist `requireAdministrator` gesetzt – beim Debuggen startet Visual Studio/Cursor die App ggf. mit Nachfrage nach Adminrechten.

## Daten

| Datei | Pfad |
|--------|------|
| Profile | `%AppData%\ProfileIpSwitcher\profiles.json` |
| Einstellungen | `%AppData%\ProfileIpSwitcher\settings.json` |
| Log | `%AppData%\ProfileIpSwitcher\logs\app.log` |

## Beispiel `profiles.json`

Siehe `examples\profiles.example.json`.

## Beispiel `settings.json`

Siehe `examples\settings.example.json`.

## Abhängigkeiten

- **MahApps.Metro** – UI-Theme  
- **System.Management** – WMI (Netzwerkprofil u. a.)  
- **Windows Forms** – nur für `NotifyIcon` (Infobereich)

## Update-Check

`UpdateCheckService` ist ein **Stub** mit TODO für einen echten HTTP-Endpunkt (z. B. GitHub Releases API oder `version.json`).

