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

### Release-Paket lokal (portable Single-File, self-contained)

```powershell
cd IP-Changer
dotnet publish IP-Changer\IP-Changer.csproj -c Release -r win-x64 -o .\publish
```

Ergebnis: `publish\ProfileIpSwitcher.exe` (eine große, portable EXE inkl. .NET-Runtime).

## GitHub-Release (Tag + automatisches ZIP)

1. Änderungen committen und pushen (`main`).
2. **Tag setzen und pushen** (muss zum Update-Check passen, mit `v`):

   ```powershell
   git tag -a v1.0.0-alpha.2 -m "Release notes kurz"
   git push origin v1.0.0-alpha.2
   ```

3. **GitHub Actions** (Workflow [`.github/workflows/release.yml`](.github/workflows/release.yml)) baut auf `windows-latest`, erstellt/aktualisiert das **Release** zum Tag und hängt **`ProfileIpSwitcher-<Tag>-win-x64-self-contained.zip`** (nur die EXE darin) an.

**Repo-Einstellung:** unter *Settings → Actions → General → Workflow permissions* muss **Read and write permissions** erlaubt sein (sonst kann der Workflow kein Release anlegen).

**Manuell:** Du kannst weiterhin unter **Releases** von Hand Dateien anhängen; der Workflow macht es bei jedem neuen `v*`-Tag automatisch.

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
