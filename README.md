# Portal 2 BETA Launcher

<img width="308" height="80" alt="P2BL_ProgAboutLogo" src="https://github.com/user-attachments/assets/7837cad1-59b4-4213-ac55-3759615e2471" />

> **Portal 2 BETA Launcher** is a Windows launcher and utility suite for locating, launching, organizing, testing, and preserving locally stored **Portal 2 beta builds**.

---

## About

Portal 2 BETA Launcher was created to make working with Portal 2 beta builds significantly easier than manually configuring launch commands, managing folders, and maintaining separate tools for every build.

The project is designed primarily for **Portal 2 beta research, testing, documentation, and preservation**, with the long-term goal of putting the most common beta-build workflow into one application.

The launcher began as a small **R-1 prototype** and is currently being rebuilt around a substantially larger **R-2 architecture and user interface**.

## Website

There's Now a P2BL Website, where All Releases & Docs for P2BL Will be Hosted on

For More in-death info About P2BL, visit the New P2BL Website, P2BL R-2 is **STILL** in Active Development

https://sonicfantech.org/Site/P2BL.NET

### Portal Series Beta Research

Portal 2 BETA Launcher is made by a member of the **Portal Series Beta Research (PSBR)** Discord community for the benefit of the server and its members.

<img width="128" height="128" alt="PSBR_Logo" src="https://github.com/user-attachments/assets/cd2d89e5-67ff-4e21-bca5-3f74497d7812" />

**PSBR Discord Server:**  
https://discord.gg/ddBC3BVg5d

The PSBR server is a community focused on the research, discussion, testing, documentation, and preservation of Portal series beta content and related development material.

> **Important:** Portal 2 BETA Launcher is a community-made project. It is **not an official Valve or Portal 2 project**, and it is not affiliated with, sponsored by, or endorsed by Valve Corporation.

---

## Project Status

> **Current development target: R-2**
>
> R-2 is a major rebuild of the original prototype. The old R-1 feature set remains documented below because R-2 is **still in development** and is not yet the stable replacement for the prototype.

The interface, internal architecture, data handling, file formats, and feature organization are all being reworked as part of R-2.

---

# R-2 — In Development

R-2 is not simply a visual update. It is a full rebuild intended to turn the original prototype into a more complete Portal 2 beta management and research utility.

## New GUI
<img width="1920" height="1052" alt="image" src="https://github.com/user-attachments/assets/2c0fb524-822f-40e5-8975-0e703cc5e4ba" />


## R-2 Feature Set

### Full UI Rebuild

The original prototype interface is being replaced by a completely redesigned Windows UI.

The R-2 interface is being designed around:

- A Valve-inspired visual style
- A structured sidebar/navigation system
- Expandable navigation categories and sub-pages
- Dedicated feature tabs instead of one large prototype-style layout
- Improved spacing, alignment, scaling, and organization
- Better About and Settings pages
- Scrollable navigation where required
- Cleaner separation between launcher, management, debugging, and utility features
- Progress indicators for operations that may take time

The R-2 layout is intended to make the launcher feel like a complete application rather than a collection of prototype controls.

---

### Beta Build Scanner

R-2 continues the original beta-build scanning system, with a stronger focus on speed, reliability, and visibility.

Planned capabilities include:

- Scanning connected drives for known Portal 2 beta builds
- Scanning user-selected folders
- Maintaining a list of custom scan locations
- Detecting recognized build folders automatically
- Displaying scan progress
- Reporting the current scan state instead of leaving the UI appearing inactive
- Managing discovered builds through the Build Manager

---

### Build Manager

The R-2 Build Manager is intended to become the central location for working with installed beta builds.

Planned functionality includes:

- Viewing detected beta builds
- Switching between build-management layouts
- Build information and metadata
- Launching a selected build
- Managing build-specific settings
- Build-specific content paths
- Extensible management for additional beta-related tools

The original prototype's build list remains part of the R-1 legacy feature set below.

---

### Launch Profiles & Launch Arguments

R-2 is intended to make launch configuration easier and more flexible.

Planned support includes:

- Build-specific launch arguments
- Optional map launching
- Windowed launch options
- Resolution configuration
- Steam startup handling
- Extra/custom launch arguments
- Easier editing of launch parameters
- Saved launch configurations

A dedicated **Extra Launch Args** area is planned for advanced and debugging-oriented launch customization.

---

### Screenshot System

R-2 includes a planned screenshot system designed around the launcher rather than relying entirely on external software.

The system is intended to provide:

- A launcher-managed screenshot directory
- Screenshot history/listing
- Viewing previously captured screenshots
- Organized screenshot metadata
- A dedicated in-game screenshot interface
- A launcher overlay inspired by the convenience of the Steam Overlay, but designed specifically for Portal 2 beta builds
- A way to open the P2BL overlay while a supported beta build is running

The screenshot feature is being developed as part of the larger R-2 utility/overlay system.

---

### Custom Map Manager

R-2 is planned to include a dedicated **Custom Map Manager** for managing map content independently for each beta build.

The planned layout uses build-specific content directories such as:

```text
Resources/
└── C-Maps/
    └── <build>/
```

The exact map-management workflow is still being developed.

---

### Patch Manager

A dedicated **Patch Manager** is planned for R-2.

This area is intended to provide a central place for build-specific patches and compatibility changes.

> **Status:** R-2 placeholder / in development.

---

### Mod Manager

A dedicated **Mod Manager** is also planned.

The intended goal is to eventually make build-specific mod organization easier while keeping the files separated by beta build.

The planned structure follows the same general approach as custom maps:

```text
Resources/
└── Mods/
    └── <build>/
```

> **Status:** R-2 placeholder / in development.

---

### Debugger

R-2 is planned to include a dedicated debugger-oriented section rather than treating debugging as a separate console-only utility.

The planned debugger is intended to provide a Task-Manager-like view for Portal 2 beta-related processes, including capabilities such as:

- Viewing relevant running processes
- PID display
- Starting a task/program
- Ending a task
- Process details/properties
- Process icons
- Debugging-oriented controls
- Dedicated debugger sub-pages

The debugger is focused on **Portal 2 beta processes and related tools**, not general system-performance monitoring.

---

### Steam Integration

The original Steam integration remains part of the project and is being carried forward into the R-2 architecture.

Current/prototype behavior includes:

- Adding detected beta builds to Steam as shortcuts
- Backing up `shortcuts.vdf` before making changes

---

### Bundled Fixes

The launcher can work with bundled fixes intended for Portal 2 beta builds that need additional files or compatibility changes.

The prototype behavior includes:

- Applying bundled fixes
- Backing up files before changes are made

R-2 is intended to make this functionality easier to manage through the rebuilt UI.

---

### Port-Forwarding Helper

The original utility for Windows TCP port proxies is retained as part of the launcher feature set.

It is intended for beta setups that require custom local networking or port-proxy configuration.

---

### Settings & Saved Data

R-2 is being designed around dedicated launcher-owned configuration and saved-data handling rather than scattering settings throughout the UI.

The project already uses launcher-specific saved-data locations, with the R-2 architecture continuing to expand this system.

---

### Custom File Types & Data Formats

R-2 is planned to introduce and/or expand support for **custom launcher-owned file types and data formats**.

These formats are intended to make launcher data more structured, portable, and easier for the application to manage.

The R-2 data system includes planned use of custom formats such as:

```text
.SSDA
```

The exact file specifications are part of the ongoing R-2 development and may change before release.

---

### Progress & Operation Feedback

R-2 is being designed so that long-running tasks visibly report what they are doing.

This includes planned progress information for operations such as:

- Drive scanning
- Folder scanning
- Build detection
- Install/copy operations
- Fix installation
- Other potentially long-running launcher tasks

---

# R-1 Prototype — Legacy / Existing Feature Set

The original R-1 prototype is still important because it represents the foundation of the project and may remain useful while R-2 is being completed.

## Legacy GUI
<img width="1365" height="857" alt="image" src="https://github.com/user-attachments/assets/497d0434-9726-4067-84a4-453fe8f37e1a" />


## R-1 Features

### Beta Build Scanner

- Scan connected drives for known Portal 2 beta builds.
- Scan manually added folders.
- Keep a list of custom scan folders.

### Build Launcher

- Launch detected beta builds directly.
- Optional map selection.
- Configurable window width and height.
- Option to start Steam before launching a build.

### Steam Integration

- Add detected beta builds to Steam as shortcuts.
- Back up `shortcuts.vdf` before making changes.

### Bundled Fixes

- Includes bundled fixes intended for beta builds that need additional files or compatibility changes.
- Existing files are backed up before fixes are installed.

### Port-Forwarding Helper

- Add, remove, and list Windows TCP port proxies for Portal 2 beta-related setups.

### GUI + Console Modes

- Windows Forms GUI for normal use.
- Console interface for advanced/manual operation.

### Debug Console

- Attach a separate debug console to a running launcher process.

# Known Portal 2 Beta Builds

The launcher is designed around known Portal 2 beta builds, including the following build identifiers currently used by the project:

| Build | Date / Period |
|---|---|
| `852_0` | July 2009 |
| `841_0` | February 2010 |
| `852_1` | March 2010 |
| `852_2` | July 2010 |
| `852_3` | August 2010 |
| `841_1` | September 24, 2010 |
| `841_2` | September 26, 2010 |
| `852_4` | October 2010 |
| `852_6` | December 2010 |
| `841_3` | January 2011 |

> Build support may expand as additional research and preservation work is completed.

---

# Requirements

For the current R-1 prototype/build environment:

- Windows
- .NET 8 / .NET 8 SDK for building
- x64 system
- Visual Studio 2026 or another compatible .NET 8 build environment

The project targets:

```text
net8.0-windows
```

and is configured for x64 builds.

> R-2 requirements may change as the application architecture is rebuilt.

---

# Building

Clone the repository and run:

```bat
Build.bat
```

The included build script attempts to use the installed .NET SDK and can fall back to Visual Studio MSBuild when available.

You can also open:

```text
Portal2BetaLauncher.sln
```

in Visual Studio and build the **Release / x64** configuration.

---

# Running

To open the GUI directly:

```bat
Run-GUI.bat
```

The executable can also be started with:

```text
--gui
```

The prototype can run in console mode when `--gui` is not used.

> R-2 is being rebuilt with the GUI as the primary experience.

---

# Project Structure

The R-1 prototype currently uses a structure similar to:

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

R-2 introduces a broader application architecture and additional resource/tool directories. The project structure is expected to continue changing while R-2 is under active development.

---

# Beta Research & Preservation

Portal 2 BETA Launcher is intended for people researching, testing, documenting, and preserving Portal 2 beta builds.

The project is particularly useful when working with builds that require:

- Custom launch arguments
- Additional compatibility files
- Steam shortcuts
- Build-specific content
- Custom maps or mods
- Debugging tools
- Preservation-oriented file management

The goal is to reduce the amount of manual setup required when working with older Portal 2 development builds.

---

# Contributing & Testing

Because R-2 is a major rebuild, testing and bug reports are especially valuable during development.

For useful bug reports, include:

- The Portal 2 beta build being used
- Your Windows version
- What you were trying to do
- Any error message or console output
- Steps needed to reproduce the issue
- Relevant screenshots when applicable

Source-code availability may differ by release and licensing model. See the **License & Source Availability** section below.

---

# License & Source Availability

## R-1 Prototype

The original R-1 prototype repository is currently licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

See [`LICENSE`](LICENSE) for the current repository license.

## R-2 Licensing

A **custom EULA / closed-source licensing model is being considered for R-2**.

Under the proposed R-2 model:

- The released application would be closed-source by default.
- Normal users would receive the compiled application and its permitted files, not the complete source tree.
- Certain qualified groups or individuals may be able to **request source-code access** for legitimate research, preservation, collaboration, security review, maintenance, or other approved purposes.
- Source-code requests would be reviewed individually and are **not automatically guaranteed**.
- Any approved source-code access may be subject to additional terms, restrictions, or a separate agreement.

The final licensing terms will be published with the applicable R-2 release before the new license becomes authoritative.

See [`EULA-DRAFT.md`](EULA-DRAFT.md) for the current proposed licensing model.

> **Licensing note:** Until the R-2 license is officially changed and published, the existing repository license remains the governing license for material currently distributed under it.

# Credits

Created by **sonic Fan Tech**.

Portal 2, Portal, Steam, and other Valve-related names and properties are owned by their respective rights holders.

Portal 2 BETA Launcher is an independent community project and is **not affiliated with, sponsored by, or endorsed by Valve Corporation**.
