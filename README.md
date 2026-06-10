# Agent Browser

Portable Windows managed browser bundle with:

- `Browser-only Proxy` as the default Windows runtime mode
- optional advanced `System TUN` mode
- bundled Chromium/Chrome portable runtime
- local profile directory
- minimal `Start.exe` and `Stop.exe`
- optional `AgentBrowserUI.exe` for editing connection settings
- local logs and operator diagnostics

This repository is the source and governance layer for the managed browser workspace across platforms.

The current active implementation is Windows-first. Future macOS work should live in the same source repository, but normal git history should still contain source code, docs, and build orchestration only.

## Product Positioning

This project should be treated as:

- managed browser workspace
- isolated session browser
- portable browser + tunnel controller
- operator-friendly browser runtime with presets and logs

This project should not be positioned as:

- anti-detect browser
- stealth browser
- identity spoofing tool
- “undetectable” browser

That distinction matters for both engineering scope and commercial risk.

## Current Source Of Truth

Use these projects as the active implementation:

- `AgentBrowserUi/`
- `SingBoxStart/`
- `SingBoxStop/`

Current active platform:

- Windows

Planned future platform track:

- macOS

Treat these as legacy prototypes unless you explicitly need them:

- root `Program.cs` and `Launcher.csproj`
- `ChromiumLauncher/`
- `StopXray/`

For documentation governance:

- `SOURCES_OF_TRUTH.md` defines which document or code path is authoritative per domain
- `DOCUMENT_REGISTRY.md` defines which project documents are official and active
- `DECISION_LOG.md` and `ADR/` record durable product and architecture decisions

## Repository Scope And GitHub

Default tracked repository contents:

- active source projects and shared libraries
- durable docs and governance files
- `AgentBrowser.sln` as the default solution entry point
- lightweight package manuals under `AgentBrowser_Windows/`
- future platform-specific source projects, including macOS, when they actually exist

Default excluded local artifacts:

- packaged runtime binaries under `AgentBrowser_Windows/`
- `AgentBrowser_Windows.zip`
- downloaded vendor inputs under `resurs/`
- local `bin/`, `obj/`, logs, and support bundles

Use `REPO_SETUP.md` together with `.gitignore` as the authoritative repo-boundary guide when preparing GitHub pushes or moving the working copy to an external drive.

## Platform Layout

Shared logic belongs in `libs/`.

Platform-specific implementation belongs in `platforms/<platform>/`.

Current foundation:

- `platforms/macos/` exists as a skeleton only
- `platforms/macos/app/AgentBrowser.MacShell/` is the first buildable macOS shell project
- `platforms/macos/AgentBrowser.Mac.sln` is the platform-local solution entry point
- there is not yet a real macOS runtime implementation
- Windows remains the active runtime path through the current root projects
- the selected first macOS experiment is an `Avalonia` shell, not runtime parity

## Main Components

- `AgentBrowserUi/Program.cs`: Windows UI for presets, config generation, validation, and launching helper executables
- `SingBoxStart/Program.cs`: session launcher that validates config, starts `sing-box` in the selected runtime mode, launches the bundled browser, waits for browser exit, and stops `sing-box`
- `SingBoxStop/Program.cs`: session stopper that kills bundled Chrome and `sing-box` from the same folder only, with conditional elevation only for `System TUN`
- `AgentBrowser_Windows/`: ready-to-run Windows package

## Package Layout

```text
AgentBrowser_Windows/
├── AgentBrowserUI.exe
├── Start.exe
├── Stop.exe
├── chrome.exe
├── App/
├── Data/
├── Other/
├── Profile/
├── core/
│   ├── sing-box.exe
│   ├── config.json
│   ├── wintun.dll
│   └── libcronet.dll
├── ui-settings.json
└── README.txt
```

## Logs

Runtime diagnostics are written next to the executables:

- `ui.log`: UI startup, button actions, validation failures, launch errors
- `start.log`: `Start.exe` lifecycle and browser/tunnel orchestration
- `stop.log`: `Stop.exe` lifecycle and process shutdown attempts
- `core/sing-box.log`: actual tunnel runtime log

If connectivity fails, `core/sing-box.log` is the primary source.

## Profile Portability

The browser profile folder is portable at the file level, but authentication is not guaranteed to survive transfer to another Windows machine.

Reason:

- Chromium-family browsers protect some sensitive data with Windows DPAPI or newer Windows-bound encryption layers.
- That means cookies, tokens, and stored secrets may decrypt only for the original Windows user and often only on the original machine.
- Copying `Data/` or `Profile/` to another PC can leave the visible profile intact while breaking the encrypted auth material.

Practical consequence:

- bookmarks and some profile preferences often survive
- authenticated sessions and saved secrets may not

If full cross-machine session portability becomes a product requirement, it needs a dedicated session portability design instead of assuming the bundled browser profile is enough.

## Build

All current executables target `net8.0-windows`.

Build UI:

```bash
dotnet publish AgentBrowserUi/AgentBrowserUi.csproj -c Release -r win-x64 --self-contained true
```

Build start launcher:

```bash
dotnet publish SingBoxStart/Start.csproj -c Release -r win-x64 --self-contained true
```

Build stop launcher:

```bash
dotnet publish SingBoxStop/Stop.csproj -c Release -r win-x64 --self-contained true
```

## Running On Windows

Recommended flow:

1. Launch `AgentBrowserUI.exe`
2. Choose `SOCKS5` or `VLESS`
3. Paste the connection string
4. Click `Test`
5. Click `Save`
6. Click `Start`

`Browser-only Proxy` is the default and recommended runtime mode.
`Start.exe` and `Stop.exe` run without elevation in that mode and request administrator rights only for explicit `System TUN`.

## Commercial Direction

The realistic commercial path is:

- team browser management
- isolated profiles
- preset-based proxy or tunnel control
- supportable operator UX
- auditable diagnostics

Not:

- aggressive fingerprint spoofing
- claims about invisibility or anti-detection

## Platform Scope

Repository scope:

- current Windows implementation
- future macOS implementation in the same repository

Delivery scope today:

- Windows only

## Docs Index

- `REPO_SETUP.md`
- `MACOS_RUNTIME_OPTIONS.md`
- `platforms/README.md`
- `platforms/macos/README.md`
- `RELEASE_CHECKLIST.md`
- `WINDOWS_SMOKE_CHECKLIST.md`
- `CHANGE_CHECKLIST.md`
- `PULL_REQUEST_TEMPLATE.md`
- `DECISION_LOG.md`
- `ADR/README.md`
- `SOURCES_OF_TRUTH.md`
- `DOCUMENT_REGISTRY.md`
- `ARCHITECTURE.md`
- `CONFIGURATION.md`
- `SESSION_PORTABILITY.md`
- `WORKSPACE_V2_BACKLOG.md`
- `STATUS.md`
- `ROADMAP.md`
- `DEVLOG.md`
- `AGENTS.md`
