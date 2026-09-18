# Portal 2 BETA Launcher

<img width="308" height="80" alt="P2BL_ProgAboutLogo" src="https://github.com/user-attachments/assets/7837cad1-59b4-4213-ac55-3759615e2471" />

## About

A Windows launcher and utility for working with **Portal 2 beta builds**.

Portal 2 BETA Launcher is designed to make it easier to find, launch, and manage locally stored Portal 2 beta builds without having to manually set up launch commands every time.

## Notice
This program, **Portal 2 BETA Launcher**, is being made by a Member of the **Portal Series Beta Research** [`DISCORD SERVER`]([LICENSE](https://discord.gg/ddBC3BVg5d)), for the Server & it's Members

<img width="128" height="128" alt="PSBR_Logo" src="https://github.com/user-attachments/assets/cd2d89e5-67ff-4e21-bca5-3f74497d7812" />

> **Project status:** Active development
>
> This project is a community-made utility for Portal 2 beta research and preservation. It is **not an official Valve/Portal 2 project**.

## IN DEVELOPMENT R-2 FEATURE SET & UI REWORK SCREENSHOTS

**R-2 Features: IN DEVELOPMENT**

1. PLACEHOLDER
2. PLACEHOLDER

**Screenshots**

<img width="1358" height="854" alt="image" src="https://github.com/user-attachments/assets/083ec5c9-9c45-4eef-8957-e887b9a177b8" />


## Features

- **Beta Build Scanner**
  - Scan connected drives for known Portal 2 beta builds.
  - Scan manually added folders only.
  - Keep a list of custom scan folders.

- **Build Launcher**
  - Launch detected beta builds directly.
  - Optional map selection.
  - Configurable window width and height.
  - Option to start Steam before launching a build.

- **Steam Integration**
  - Add detected beta builds to Steam as shortcuts.
  - Backs up `shortcuts.vdf` before making changes.

- **Bundled Fixes**
  - Includes bundled fixes intended for beta builds that need additional files or compatibility changes.
  - Existing files are backed up before fixes are installed.

- **Port-Forwarding Helper**
  - Add, remove, and list Windows TCP port proxies for Portal 2 beta-related setups.

- **GUI + Console Modes**
  - Use the Windows Forms GUI for normal use.
  - A console interface is also available for advanced/manual operation.

- **Debug Console**
  - Attach a separate debug console to a running launcher process.

## Requirements

- Windows
- .NET 8 / .NET 8 SDK for building
- x64 system
- Visual Studio 2026 or another compatible .NET 8 build environment

The project targets `net8.0-windows` and is configured specifically for x64 builds. 

## Building

Clone the repository and run:

```bat
Build.bat
```

The included build script first tries the installed .NET SDK and then falls back to Visual Studio MSBuild when available. 

You can also open `Portal2BetaLauncher.sln` in Visual Studio and build the **Release / x64** configuration.

## Running

To open the GUI directly, run:

```bat
Run-GUI.bat
```

The executable can also be started with the `--gui` argument.

Without `--gui`, the launcher starts in console mode and provides access to scanning, launching, Steam integration, fixes, port forwarding, and other utilities. 

## Project Structure

```text
Portal-2-BETA-Launcher---Windows/
├── Build.bat
├── Run-GUI.bat
├── EmbeddedFixes.zip
├── LICENSE
├── Portal2BetaLauncher.sln
└── Portal2BetaLauncher/
    ├── Models/
    ├── Services/
    ├── UI/
    ├── Program.cs
    ├── Portal2BetaLauncher.csproj
    └── app.manifest
```

The repository includes the launcher source, bundled fixes, solution/project files, and build/run scripts. 

## Beta Research

This launcher is primarily intended for people researching, testing, and preserving Portal 2 beta builds.

It is especially useful when working with builds that require custom launch arguments, extra compatibility files, Steam shortcuts, or other setup steps.

## Contributing

Bug reports, testing results, fixes, and improvements are welcome.

When reporting a problem, please include:

- The Portal 2 beta build being used
- Your Windows version
- What you were trying to do
- Any error message or console output
- Steps needed to reproduce the issue

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

See [`LICENSE`](LICENSE) for the full license text.

## Credits

Created by **sonic Fan Tech**.

Portal 2 and related Valve properties are trademarks of their respective owners. This project is an independent community-made tool and is not affiliated with or endorsed by Valve.
