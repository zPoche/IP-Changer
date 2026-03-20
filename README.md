# ProfileIpSwitcher

Windows-Desktop-App (.NET 8, WPF, MahApps.Metro) zum schnellen Wechsel von IPv4-/DNS-Einstellungen pro Netzwerkadapter (ähnlich wie NetSetMan, reduzierter Funktionsumfang).

**Aktuelle Version:** siehe `IP-Changer/IP-Changer.csproj` (`Version`) und [CHANGELOG.md](CHANGELOG.md).

## Build & Start

Repository-Root heißt idealerweise **`IP-Changer`** (falls der Ordner noch `ProfileIpSwitcher` heißt: Cursor schließen, Ordner in `IP-Changer` umbenennen, Projekt neu öffnen).

```powershell
cd IP-Changer
dotnet build IP-Changer.sln
dotnet run --project IP-Changer\IP-Changer.csproj
```

Die Build-Ausgabe liegt unter `IP-Changer\bin\Debug\net8.0-windows\` (bzw. `Release`). Die EXE heißt weiterhin **`ProfileIpSwitcher.exe`** (AssemblyName in der `.csproj`).  
**Hinweis:** Über `app.manifest` ist `requireAdministrator` gesetzt – beim Debuggen startet Visual Studio/Cursor die App ggf. mit Nachfrage nach Adminrechten.

### Release-Paket bauen (ZIP zum Hochladen)

```powershell
cd IP-Changer
dotnet publish IP-Changer\IP-Changer.csproj -c Release -r win-x64 --self-contained false -o .\publish
```

Ordner `publish` enthält u. a. `ProfileIpSwitcher.exe` – als ZIP packen und bei GitHub unter **Releases** anhängen.

## GitHub-Release (Alpha / Version)

1. Änderungen committen und pushen (`main`).
2. **Tag setzen** (muss zum Update-Check passen, mit `v`):

   ```powershell
   git tag -a v1.0.0-alpha.1 -m "Alpha 1.0"
   git push origin v1.0.0-alpha.1
   ```

3. Auf GitHub: **Releases → Draft a new release** → Tag `v1.0.0-alpha.1` wählen, Titel z. B. `1.0.0-alpha.1`, Beschreibung aus `CHANGELOG.md`, **ZIP aus `publish`** anhängen, veröffentlichen.

Die App vergleicht die installierte Assembly-Version mit dem **neuesten Release-Tag** auf GitHub.

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

Unter **Extras → Nach Updates suchen** wird die GitHub-**Releases**-API aufgerufen (Repo-URL in den Einstellungen, Standard: `https://github.com/…/IP-Changer`). Es muss mindestens ein **Release** mit Tag (z. B. `v1.0.0-alpha.2`) existieren.  
Alternativ: direkte JSON-URL (z. B. `version.json` auf `raw.githubusercontent.com`) mit Feld `latestVersion`.

- Beim Start: optional (Häkchen in den Einstellungen); bei neuem Release erscheint ein Hinweis mit Option, die Release-Seite zu öffnen.
- Der Versionsvergleich nutzt die numerische **AssemblyVersion** (z. B. `1.0.0.0`); Pre-Release-Suffixe im GitHub-Tag werden für den Vergleich auf `Major.Minor.Build` gekürzt.
