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

## Auf GitHub veröffentlichen

1. **Git-Nutzer setzen** (einmalig, oder nur in diesem Repo ohne `--global`):

   ```powershell
   git config --global user.name "Dein Name"
   git config --global user.email "deine@email.de"
   ```

2. **Ersten Commit** (im Ordner `ProfileIpSwitcher`, Dateien sind schon per `git add` vorgemerkt):

   ```powershell
   cd "c:\Users\Poche\Documents\# Coding\Erstes Projekt\ProfileIpSwitcher"
   git commit -m "Initial commit: ProfileIpSwitcher"
   ```

3. **Neues Repository auf GitHub**  
   Auf [github.com/new](https://github.com/new) ein leeres Repo anlegen (ohne README/License, damit es keine Konflikte gibt).

4. **Remote hinzufügen und pushen** (`DEINUSER` und `ProfileIpSwitcher` anpassen):

   ```powershell
   git branch -M main
   git remote add origin https://github.com/DEINUSER/ProfileIpSwitcher.git
   git push -u origin main
   ```

   Bei HTTPS fragt Git nach Anmeldung (Personal Access Token statt Passwort) oder du nutzt **GitHub Desktop** / **SSH**.
