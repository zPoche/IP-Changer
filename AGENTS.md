# AGENTS.md

## Cursor Cloud specific instructions

### Project overview

IP-Changer (ProfileIpSwitcher) is a single-project Windows desktop WPF application (.NET 8, MahApps.Metro) for switching IPv4/DNS profiles per network adapter. See `README.md` for full details.

### Build & lint on Linux

The project targets `net8.0-windows` (WPF). On Linux you **must** pass `-p:EnableWindowsTargeting=true` to all `dotnet` commands:

```sh
dotnet restore IP-Changer.sln -p:EnableWindowsTargeting=true
dotnet build IP-Changer.sln -p:EnableWindowsTargeting=true
dotnet build IP-Changer.sln -p:EnableWindowsTargeting=true -warnaserror   # lint-style check
```

There is no `.editorconfig`, no Roslyn analyzer config, and no separate linter. The `-warnaserror` build is the closest lint equivalent.

### Tests

There are **no test projects** in this repository. The codebase has no unit or integration tests.

### Running the application

The compiled binary (`ProfileIpSwitcher.exe`) **cannot run on Linux** because it uses WPF, Windows Forms `NotifyIcon`, WMI (`Win32_NetworkAdapterConfiguration`), and `netsh`. Running the app requires a Windows machine with administrator privileges and network adapters.

### Key paths

| What | Path |
|------|------|
| Solution | `IP-Changer.sln` |
| Project | `IP-Changer/IP-Changer.csproj` |
| Build output (Debug) | `IP-Changer/bin/Debug/net8.0-windows/win-x64/` |
| Example profiles | `examples/profiles.example.json` |
| Example settings | `examples/settings.example.json` |
| CI workflow | `.github/workflows/release.yml` |

### .NET SDK

The update script installs .NET 8 SDK to `$HOME/.dotnet` and adds it to `PATH`. If `dotnet` is not found, ensure `$HOME/.dotnet` is on `PATH`.
