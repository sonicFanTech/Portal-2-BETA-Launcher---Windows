# Portal 2 BETA Launcher

<img width="308" height="80" alt="P2BL_ProgAboutLogo" src="https://github.com/user-attachments/assets/7837cad1-59b4-4213-ac55-3759615e2471" />

> **Portal 2 BETA Launcher (P2BL)** is a Windows launcher and utility suite for locating, launching, organizing, testing, and preserving locally stored **Portal 2 beta builds**.

---

## Current Release

**R-2 BETA — `v2.025.2-9.29.201-B`**

R-2 is the current development line of Portal 2 BETA Launcher and is a major rebuild of the original prototype. It introduces a substantially redesigned GUI, a larger build-management architecture, improved scanning workflows, build-specific content organization, and the foundation for additional research and preservation utilities.

> **R-2 is still a Beta and remains under active development.** Some systems are complete and usable, while others are still being implemented, tested, or redesigned.

For the complete feature/changelog information for this release, see:

**[`P2BL_R-2_BETA_v2.025.2-9.29.201-B_Features_and_Bug_Fixes.md`](P2BL_R-2_BETA_v2.025.2-9.29.201-B_Features_and_Bug_Fixes.md)**

---

## Website

The official P2BL website is the main place for release information, documentation, downloads, and additional project information.

**P2BL Website:**

https://sonicfantech.org/Site/P2BL.NET

R-2 remains in active development, so the website and release documentation may continue to change as new Beta builds are published.

---

## Portal Series Beta Research

Portal 2 BETA Launcher is made by a member of the **Portal Series Beta Research (PSBR)** community for the benefit of beta researchers, testers, preservationists, and interested community members.

<img width="128" height="128" alt="PSBR_Logo" src="https://github.com/user-attachments/assets/cd2d89e5-67ff-4e21-bca5-3f74497d7812" />

**PSBR Discord Server:**

The PSBR Discord invitation is available from the **About** section inside P2BL R-2.

The PSBR community focuses on the research, discussion, testing, documentation, and preservation of Portal series beta content and related development material.

> **Important:** Portal 2 BETA Launcher is a community-made project. It is **not an official Valve or Portal 2 project**, and it is not affiliated with, sponsored by, or endorsed by Valve Corporation.

---

# R-2 BETA Feature Overview

## Rebuilt R-2 GUI

<img width="1920" height="1052" alt="P2BL R-2 GUI" src="https://github.com/user-attachments/assets/2c0fb524-822f-40e5-8975-0e703cc5e4ba" />

R-2 replaces the original prototype-style interface with a redesigned Windows GUI focused on easier navigation and separation of the launcher, build management, utilities, debugging, settings, and other tools.

### Current GUI Features

- Valve-inspired visual direction
- Structured left-side navigation
- Dedicated pages and sections
- Expandable/organized navigation categories
- Improved spacing and layout organization
- Dedicated About and Settings areas
- Scrollable navigation where needed
- GUI-first workflow
- More visible progress and operation feedback

---

## Beta Build Scanner

The R-2 scanner is designed to locate known Portal 2 beta builds across storage locations and make discovered builds available to the launcher.

### Features

- Scan connected drives
- Scan user-accessible folders
- Scan selected/custom locations
- Detect recognized Portal 2 beta build layouts
- Display scan progress/status
- Display discovered builds in the launcher
- Integrate detected builds with the Build Manager
- Continue improving duplicate and repack handling

The scanner is also being reworked so that valid builds can be added to the launcher list **as soon as they are discovered**, rather than waiting for an entire scan to finish.

---

## Build Manager

The **Build Manager** is the central R-2 system for working with detected beta builds.

### Current / Active Features

- Detected beta build list
- Build display information
- Build-specific data
- Selected-build workflow
- Launch integration
- Foundation for additional build-specific tools

The Build Manager is also the foundation for the ongoing **New Builds Manager** work.

---

## Beta Build Launcher

P2BL can launch supported Portal 2 beta builds directly from the launcher.

### Launch Features

- Launch selected beta builds
- Build-specific executable handling
- Optional map launching
- Windowed launch options
- Steam-aware launch handling
- Build-specific launch commands
- Launch-status feedback
- Integration with custom/extra launch arguments

Common launch patterns used by the project include commands such as:

```text
hl2.exe -tempcontent -console -steam -windowed
hl2.exe -game portal2 -tempcontent -console -windowed
portal2.exe -console -steam -windowed
+map %MAP%
```

---

## Extra Launch Arguments

R-2 includes support for advanced launch customization and is continuing to expand its dedicated **Extra Launch Args** workflow.

### Supported / Active Direction

- Custom launch arguments
- Build-specific argument handling
- Map arguments
- Windowed-mode options
- Advanced/debugging launch customization

### Status

The dedicated advanced Extra Launch Args interface is **still being developed**.

---

## Build-Specific Content Organization

R-2 is designed to keep content separated by beta build so that files intended for one build are not mixed with another.

Planned/active structures include:

```text
Resources/
├── C-Maps/
│   └── <build>/
└── Mods/
    └── <build>/
```

---

## Settings & Saved Data

R-2 uses launcher-owned configuration and saved-data handling for persistent application state.

### Features / Infrastructure

- Launcher settings
- Persistent configuration data
- Build-related stored data
- Dedicated resource/data locations
- R-2-specific data organization

---

## Custom File Types & Data Formats

R-2 is expanding P2BL's use of structured launcher-specific data formats.

One active/planned format is:

```text
.SSDA
```

Additional P2BL-specific formats and supporting projects are part of the larger R-2 architecture. Their specifications may change during development.

---

## Progress & Operation Feedback

R-2 is designed to provide more visible feedback during longer operations.

Examples include:

- Drive and folder scanning
- Build detection
- Install/copy operations
- Fix installation
- Other potentially long-running launcher tasks

---

## Steam Integration

P2BL retains Steam-related support as part of the R-2 launcher workflow.

### Features

- Add supported beta builds to Steam as shortcuts
- Back up `shortcuts.vdf` before modifying it
- Steam-aware launch handling where required

---

## Bundled Fix Support

P2BL can work with bundled fixes for supported Portal 2 beta builds.

### Features

- Bundled fix support
- File backups before modification
- Build-specific fix handling
- No-Steam compatibility/fix workflow inherited by the R-2 architecture

---

## Port-Forwarding Helper

The P2BL port-forwarding utility provides Windows TCP port-proxy management for beta setups that require custom local networking/proxy configuration.

### Features

- Add Windows TCP port proxies
- Remove existing proxies
- List configured proxies

---

# Systems Still in Development

The following R-2 systems are actively being worked on and are **not yet considered final**.

## Screenshot / In-Game Overlay System

**Status: 🔧 Active Development**

The screenshot and in-game overlay system is one of the major R-2 systems still under active development.

The goal is to provide P2BL with an in-game utility layer for supported beta builds, including:

- In-game P2BL overlay
- Screenshot capture
- Screenshot management
- Dedicated screenshot storage
- Screenshot history/listing
- Screenshot viewing
- Overlay input/keyboard handling
- Launcher/overlay integration with supported beta builds

This system is currently experimental and may change substantially in later Beta releases.

---

## New Builds Manager

**Status: 🔧 Active Development**

The New Builds Manager is being developed around the R-2 scanner and Build Manager so newly found builds can be added and managed immediately during scanning.

---

## Build Scanner Improvements

**Status: 🔧 Active Development**

Ongoing scanner work includes:

- Faster scanning
- Reduced duplicate detections
- Better handling of differing beta build layouts
- Improved repack detection
- Better result presentation
- Immediate insertion of discovered builds

---

## Debugger

**Status: 🔧 Active Development**

R-2 is planned to include a dedicated debugger/utility section focused on Portal 2 beta-related processes and tools.

Planned capabilities include:

- Process/task visibility
- PID display
- Start-task/program controls
- End-task controls
- Process details/properties
- Process icons
- Debugging-oriented controls
- Dedicated debugger sub-pages

---

## Custom Map Manager

**Status: 🔧 In Development**

A dedicated **Custom Map Manager** is being developed around build-specific content folders such as:

```text
Resources/C-Maps/<build>/
```

---

## Patch Manager

**Status: ⏸ Deferred / Planned**

A dedicated Patch Manager is planned for R-2 for build-specific patches and compatibility changes.

---

## Mod Manager

**Status: ⏸ Deferred / Planned**

A dedicated Mod Manager is planned for build-specific mod organization using directories such as:

```text
Resources/Mods/<build>/
```

---

# Features Not Yet Added

The following planned R-2 functionality is **not fully available in this Beta**:

- Full production-ready in-game overlay
- Complete screenshot-management workflow
- Fully finished New Builds Manager
- Fully finished Custom Map Manager
- Fully finished Patch Manager
- Fully finished Mod Manager
- Final production-ready debugger
- Finalized custom data/file-format system
- Complete build-specific tool integration for every planned R-2 utility
- Final advanced launch-argument workflow
- Complete support for every known Portal 2 beta repack/layout

Planned functionality may change as R-2 development continues.

---

# Known Portal 2 Beta Builds

The current P2BL build database/workflow includes these known build identifiers:

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

Build support may expand as additional beta research and preservation work continues.

---

# Known Issues / Beta Limitations

Because this is a Beta release:

- Some R-2 pages may contain unfinished functionality.
- Some controls may still be placeholders or partially implemented.
- Some systems may only work with certain beta builds.
- The screenshot/overlay system is experimental.
- Screenshot functionality is not final.
- Some scanner layouts may still require additional testing.
- Some repacked beta builds may still not be detected correctly.
- Build-specific tools are not all complete.
- Some R-2 systems are still being migrated or redesigned.
- Internal data formats may change between Beta releases.

For detailed development issues and fixes, see the release feature/changelog document.

---

# Release Notes & Bug Fixes

For the complete feature list, bug fixes, in-development systems, planned features, and Beta limitations for this release, see:

**[`P2BL_R-2_BETA_v2.025.2-9.29.201-B_Features_and_Bug_Fixes.md`](P2BL_R-2_BETA_v2.025.2-9.29.201-B_Features_and_Bug_Fixes.md)**

---

# Requirements

For **P2BL R-2 BETA v2.025.2-9.29.201-B**:

- Windows x64
- .NET 8 runtime / compatible .NET 8 environment
- x64-capable system

The project uses the Windows-targeted .NET 8 application architecture used by the R-2 development builds.

---

# Building

R-2 is a development build and its project structure may continue to change between Beta releases.

For a source build, open the included Visual Studio solution and build the appropriate **Release / x64** configuration.

> Build instructions and project paths may change while R-2 is under active development.

---

# Beta Research & Preservation

Portal 2 BETA Launcher is intended for people researching, testing, documenting, and preserving Portal 2 beta builds.

The project is particularly useful for workflows involving:

- Custom launch arguments
- Compatibility files and fixes
- Steam shortcuts
- Build-specific content
- Custom maps and mods
- Debugging tools
- Screenshot and overlay tooling
- Preservation-oriented file management

The goal is to reduce the amount of manual setup required when working with older Portal 2 development builds.

---

# Reporting Bugs

R-2 is a Beta, so bug reports and testing feedback are especially useful.

When reporting a bug, include:

- P2BL version
- Portal 2 beta build being used
- Windows version
- What you were trying to do
- Exact error message, if any
- Steps to reproduce the issue
- Screenshots or recordings when useful

For scanner issues, also include the layout/type of the beta build or repack whenever possible.

---

# License

P2BL R-2 is distributed under the **R-2 EULA** included with this release.

The R-2 EULA replaces the older R-1 GPL-3.0 release terms for the R-2 distribution.

See the repository's R-2 EULA file for the complete terms.

> The EULA included with the R-2 release is the authoritative license for the R-2 distribution. Third-party components and separately licensed materials remain subject to their own applicable licenses or EULAs.

---

# Credits

Created by **sonic Fan Tech**.

Portal 2, Portal, Steam, and other Valve-related names and properties are owned by their respective rights holders.

Portal 2 BETA Launcher is an independent community project and is **not affiliated with, sponsored by, or endorsed by Valve Corporation**.

---

## Links

**P2BL Website:**
https://sonicfantech.org/Site/P2BL.NET

**GitHub Repository:**
https://github.com/sonicFanTech/Portal-2-BETA-Launcher---Windows
