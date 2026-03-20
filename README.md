# ProfileIpSwitcher

Windows-Desktop-App (.NET 8, WPF, MahApps.Metro) zum schnellen Wechsel von IPv4-/DNS-Einstellungen pro Netzwerkadapter (ähnlich wie NetSetMan, reduzierter Funktionsumfang).

## Build & Start

Repository-Root heißt idealerweise **`IP-Changer`** (falls der Ordner noch `ProfileIpSwitcher` heißt: Cursor schließen, Ordner in `IP-Changer` umbenennen, Projekt neu öffnen).

```powershell
cd IP-Changer
dotnet build IP-Changer.sln
dotnet run --project IP-Changer\IP-Changer.csproj
```

Die Build-Ausgabe liegt unter `IP-Changer\bin\Debug\net8.0-windows\` (bzw. `Release`). Die EXE heißt weiterhin **`ProfileIpSwitcher.exe`** (AssemblyName in der `.csproj`).  
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

## Updates

Unter **Extras → Nach Updates suchen** wird die GitHub-**Releases**-API aufgerufen (Repo-URL in den Einstellungen, Standard: `https://github.com/…/IP-Changer`). Es muss mindestens ein **Release** mit Tag (z. B. `v1.0.1`) existieren.  
Alternativ: direkte JSON-URL (z. B. `version.json` auf `raw.githubusercontent.com`) mit Feld `latestVersion`.

<<<<<<< Updated upstream
=======
- Beim Start: optional (Häkchen in den Einstellungen); bei neuem Release erscheint ein Hinweis mit Option, die Release-Seite zu öffnen.
- Die installierte Version entspricht der Assembly-Version in der `.csproj` (`Version` / `AssemblyVersion`).
>>>>>>> Stashed changes
