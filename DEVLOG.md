# DevLog

## Overview

This file records major project turns and why the architecture changed.

## Entry Rule

After every source or package-facing change, append a new entry to this file.

Each entry should include:

- local date and time
- reason for the change
- files edited
- package notes if binaries or archives were refreshed

## Phase 1: Firefox + Xray Prototype

- initial target was a minimal `launcher.exe`
- launcher started `Xray\\xray.exe`
- launcher then started `FirefoxPortable.exe`
- missing-file checks and `MessageBox` handling were added
- later a `stop_xray.exe` helper was introduced

Reason for change:

- Firefox portable packaging issues
- `Xray` config sensitivity
- weaker fit for the browser-control goal

## Phase 2: Chromium + Xray Prototype

- browser target moved from Firefox to Chromium
- package switched to portable Chromium runtime
- launcher logic stayed similar: start tunnel, wait, launch browser

Reason for change:

- browser compatibility goal was better with Chromium
- Xray-based packaging still remained more brittle than desired

## Phase 3: `sing-box` TUN Architecture

- architecture moved from explicit local SOCKS browser routing to `sing-box` `TUN`
- `Start.exe` and `Stop.exe` became the main helper executables
- `chrome.exe` portable bundle was used with a local `Profile/`
- admin manifest was added because `TUN` requires elevation

Reason for change:

- more robust tunnel model for the intended browsing workflow
- better fit than `v2rayN` for a portable package

## Phase 4: Minimal UI

- `AgentBrowserUI.exe` was added
- UI gained fields for connection string and connection type
- UI began generating `core/config.json`
- UI added `Save`, `Test`, `Start`, and `Stop`
- UI added preset storage in `ui-settings.json`
- UI added runtime status

Reason for change:

- avoid manual config file editing
- make switching between SOCKS and VLESS feasible for non-developer usage

## Phase 5: Diagnostics

- `start.log` and `stop.log` were expanded
- UI logging was added via `ui.log`
- package debugging now relies on layered logs instead of behavior only

Current direction:

- stabilize runtime behavior
- improve diagnostics and operator clarity
- keep the workflow to a small number of visible controls
- treat the product as a managed browser workspace rather than an anti-detect browser

## 2026-04-18 09:37:15 +09

Reason for change:

- reduce operator exposure to technical connection details
- make runtime connectivity easier to understand at a glance
- allow deleting saved presets without manual file edits
- formalize the rule that every future change must be logged in `DEVLOG.md`

Files edited:

- `AgentBrowserUi/Program.cs`
- `AgentBrowser_Windows/README.txt`
- `AGENTS.md`
- `DEVLOG.md`

Package notes:

- `AgentBrowserUI.exe` was rebuilt
- `AgentBrowser_Windows.zip` was refreshed after the UI update

Change summary:

- added `Basic / Pro` UI mode switch
- added preset deletion button
- added connection health lamp with live probe and exit IP text
- documented mandatory `DEVLOG.md` entry discipline for future edits

## 2026-04-18 09:47:10 +09

Reason for change:

- finish the last MVP usability layer for operators
- keep secrets hidden in Basic mode
- reduce accidental preset editing while still allowing simple preset management
- optionally gate Pro mode behind a flag or password-based unlock

Files edited:

- `AgentBrowserUi/Program.cs`
- `AgentBrowser_Windows/README.txt`
- `DEVLOG.md`

Package notes:

- `AgentBrowserUI.exe` was rebuilt
- `AgentBrowser_Windows.zip` was refreshed after packaging

Change summary:

- added masked connection summary in Basic mode
- added `Duplicate` and `Rename` preset buttons
- added Pro mode lock via `pro-mode.flag` or `AGENT_BROWSER_PRO_PASSWORD`

## 2026-04-18 09:51:23 +09

Reason for change:

- add short operator-facing manuals for the MVP package
- separate everyday usage guidance from technical admin guidance

Files edited:

- `AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- `AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- `AgentBrowser_Windows/README.txt`
- `DEVLOG.md`

Package notes:

- `AgentBrowser_Windows.zip` was refreshed after packaging

Change summary:

- added a short Basic manual for operators
- added a short Pro manual for admins
- linked both manuals from the package README

## 2026-04-18 09:56:05 +09

Reason for change:

- introduce a formal document registry
- define explicit Sources Of Truth by domain
- make documentation precedence and maintenance rules clear enough for long-term maintenance

Files edited:

- `DOCUMENT_REGISTRY.md`
- `SOURCES_OF_TRUTH.md`
- `README.md`
- `AGENTS.md`
- `DEVLOG.md`

Package notes:

- no package binaries changed
- no Windows runtime artifacts needed refresh for this governance-only change

Change summary:

- added a document registry for all active human-authored project docs
- added a source-of-truth map with conflict resolution rules
- linked the new governance docs from the repository entry points

## 2026-04-18 09:58:08 +09

Reason for change:

- add a formal decision log
- introduce ADRs for durable architecture and product decisions
- complete the documentation governance set with a decision layer

Files edited:

- `DECISION_LOG.md`
- `ADR/README.md`
- `ADR/ADR-0001-managed-browser-workspace-positioning.md`
- `ADR/ADR-0002-sing-box-tun-runtime.md`
- `ADR/ADR-0003-documentation-governance.md`
- `ADR/ADR-0004-basic-pro-ui-modes.md`
- `DOCUMENT_REGISTRY.md`
- `SOURCES_OF_TRUTH.md`
- `README.md`
- `AGENTS.md`
- `DEVLOG.md`

Package notes:

- no package binaries changed
- no Windows runtime artifacts needed refresh for this documentation-only change

Change summary:

- added a formal decision log
- added ADR conventions and four seed ADRs
- linked the decision layer into the registry, source-of-truth map, and contributor rules

## 2026-04-18 10:06:01 +09

Reason for change:

- replace the confusing in-window `Basic / Pro` switch with a real operator screen and separate admin settings
- make the package safer for handoff to non-technical operators
- localize visible UI labels by system language
- align package manuals and ADRs with the actual access model

Files edited:

- `AgentBrowserUi/Program.cs`
- `AgentBrowser_Windows/README.txt`
- `AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- `AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- `ADR/ADR-0004-basic-pro-ui-modes.md`
- `ADR/ADR-0006-operator-main-screen-and-locked-settings.md`
- `DECISION_LOG.md`
- `DOCUMENT_REGISTRY.md`
- `DEVLOG.md`

Package notes:

- `AgentBrowserUI.exe` was rebuilt
- `AgentBrowser_Windows` folder was updated in place
- `AgentBrowser_Windows.zip` could not be refreshed on this machine because the disk had only ~173 MiB free while the package folder is ~962 MiB

Change summary:

- removed the operator-facing `Basic / Pro` dropdown from the main window
- made the main window operator-only with `Test`, `Start`, `Stop`, runtime state, and connection health
- moved all preset editing into a separate `Settings` window
- kept admin gating through `pro-mode.flag` or `AGENT_BROWSER_PRO_PASSWORD`, but attached it to `Settings`
- added basic `ru / en` localization for visible UI text

## 2026-04-18 10:07:12 +09

Reason for change:

- add a reusable ADR template
- define an explicit threshold for when a new ADR is required

Files edited:

- `ADR/ADR-TEMPLATE.md`
- `ADR/README.md`
- `DOCUMENT_REGISTRY.md`
- `AGENTS.md`
- `DEVLOG.md`

Package notes:

- no package binaries changed
- no Windows runtime artifacts needed refresh for this documentation-only change

Change summary:

- added an ADR template
- documented when a new ADR is required vs not required
- linked the ADR trigger rule into contributor guidance

## 2026-04-18 10:08:03 +09

Reason for change:

- add explicit release and change completion checklists
- add a reusable PR/change summary template
- enforce completion gates through the governance layer

Files edited:

- `RELEASE_CHECKLIST.md`
- `CHANGE_CHECKLIST.md`
- `PULL_REQUEST_TEMPLATE.md`
- `ADR/ADR-0005-release-and-change-enforcement.md`
- `DECISION_LOG.md`
- `DOCUMENT_REGISTRY.md`
- `SOURCES_OF_TRUTH.md`
- `README.md`
- `AGENTS.md`
- `DEVLOG.md`

Package notes:

- no package binaries changed
- no Windows runtime artifacts needed refresh for this documentation-only change

Change summary:

- added release and change checklists
- added a PR/change summary template
- added an ADR for checklist-based completion enforcement

## 2026-04-18 12:58:25 +09

Reason for change:

- refresh the Windows package archive after freeing enough disk space
- close the previous packaging gap recorded earlier in the log

Files edited:

- `DEVLOG.md`
- `AgentBrowser_Windows.zip`

Package notes:

- `AgentBrowser_Windows.zip` was rebuilt successfully
- resulting archive size is approximately `423M`
- no source files changed in this packaging-only step

Change summary:

- rebuilt the distributable zip from the current `AgentBrowser_Windows` folder
- package now contains the new operator main screen and locked admin settings UI

## 2026-04-18 13:09:37 +09

Reason for change:

- prepare the workspace to become a clean GitHub source repository
- separate source history from packaged runtime artifacts and downloaded vendor bundles
- add a default solution entry point for active development

Files edited:

- `.gitignore`
- `.gitattributes`
- `REPO_SETUP.md`
- `ADR/ADR-0007-source-repository-excludes-runtime-artifacts.md`
- `README.md`
- `AGENTS.md`
- `SOURCES_OF_TRUTH.md`
- `DOCUMENT_REGISTRY.md`
- `DECISION_LOG.md`
- `DEVLOG.md`
- `AgentBrowser.sln`

Package notes:

- no packaged binaries were rebuilt
- no Windows runtime artifacts were refreshed in this change
- repository preparation intentionally excludes `AgentBrowser_Windows/`, `AgentBrowser_Windows.zip`, and `resurs/` from normal git history

Change summary:

- added repo boundary rules and git ignore policy
- added a formal ADR for source/artifact separation
- prepared a default solution file for the active projects
- initialized `git` with `main` as the default branch
- validated the default solution with `dotnet build AgentBrowser.sln -c Release`

## 2026-04-18 13:21:16 +09

Reason for change:

- define repository scope before the first GitHub push
- keep one source repository for the current Windows product and future macOS work
- state clearly that macOS is planned, but not yet a delivered runtime

Files edited:

- `ADR/ADR-0008-single-source-repo-for-windows-and-future-macos.md`
- `README.md`
- `ROADMAP.md`
- `STATUS.md`
- `AGENTS.md`
- `REPO_SETUP.md`
- `SOURCES_OF_TRUTH.md`
- `DOCUMENT_REGISTRY.md`
- `DECISION_LOG.md`
- `DEVLOG.md`

Package notes:

- no packaged binaries were rebuilt
- no Windows runtime artifacts were refreshed in this change
- this was a source-repository and governance update only

Change summary:

- accepted a single-repository strategy for Windows now and macOS later
- updated roadmap and status to treat macOS as planned rather than delivered
- prepared the repository narrative for the first GitHub push

## 2026-04-18 13:57:16 +09

Reason for change:

- prepare a real macOS foundation branch without committing to a runtime implementation yet
- define stable platform naming and folder boundaries before macOS code starts
- externalize heavy release artifacts out of the working repository tree

Files edited:

- `ADR/ADR-0009-platform-specific-source-lives-under-platforms.md`
- `platforms/README.md`
- `platforms/macos/README.md`
- `platforms/macos/app/.gitkeep`
- `platforms/macos/runtime/.gitkeep`
- `platforms/macos/packaging/.gitkeep`
- `README.md`
- `ARCHITECTURE.md`
- `ROADMAP.md`
- `STATUS.md`
- `AGENTS.md`
- `REPO_SETUP.md`
- `SOURCES_OF_TRUTH.md`
- `DOCUMENT_REGISTRY.md`
- `DECISION_LOG.md`
- `DEVLOG.md`

Package notes:

- no packaged binaries were rebuilt
- no Windows runtime artifacts were refreshed in this change
- heavy release artifacts were moved out of the repo working tree into external storage

Change summary:

- created a platform skeleton for future macOS work under `platforms/macos/`
- accepted a stable platform layout rule around `platforms/<platform>` and shared `libs/`
- prepared the repository to stay source-focused while runtime artifacts live on external storage

## 2026-04-18 14:22:27 +09

Reason for change:

- choose the first real macOS experiment instead of leaving the foundation branch stack-agnostic
- narrow the candidate stacks and reject over-ambitious first-step plans
- document the first spike as a shell-first experiment rather than runtime parity

Files edited:

- `MACOS_RUNTIME_OPTIONS.md`
- `ADR/ADR-0010-first-macos-experiment-uses-avalonia-shell.md`
- `README.md`
- `ROADMAP.md`
- `STATUS.md`
- `AGENTS.md`
- `SOURCES_OF_TRUTH.md`
- `DOCUMENT_REGISTRY.md`
- `DECISION_LOG.md`
- `DEVLOG.md`

Package notes:

- no packaged binaries were rebuilt
- no Windows runtime artifacts were refreshed in this change
- this was a macOS planning and governance update only

Change summary:

- compared the current macOS stack options and selected an `Avalonia` shell as the first experiment
- explicitly deferred tunnel, entitlement, and packaging parity work
- gave the `macos-foundation` branch a concrete next implementation target

## 2026-04-18 14:22:27 +09

Reason for change:

- convert the macOS foundation branch from documentation-only structure into a real buildable shell project
- prove that `Avalonia 12.0.1` can build on the current `.NET 8` toolchain with our shared libraries
- establish a platform-local solution entry point for future macOS work

Files edited:

- `platforms/macos/app/AgentBrowser.MacShell/AgentBrowser.MacShell.csproj`
- `platforms/macos/app/AgentBrowser.MacShell/MacShellPaths.cs`
- `platforms/macos/app/AgentBrowser.MacShell/ShellSummary.cs`
- `platforms/macos/app/AgentBrowser.MacShell/MainWindow.axaml`
- `platforms/macos/app/AgentBrowser.MacShell/MainWindow.axaml.cs`
- `platforms/macos/AgentBrowser.Mac.sln`
- `platforms/macos/README.md`
- `README.md`
- `ARCHITECTURE.md`
- `ROADMAP.md`
- `STATUS.md`
- `REPO_SETUP.md`
- `SOURCES_OF_TRUTH.md`
- `DEVLOG.md`

Package notes:

- no packaged binaries were rebuilt
- no Windows runtime artifacts were refreshed in this change
- `dotnet build platforms/macos/AgentBrowser.Mac.sln -c Release` passed successfully

Change summary:

- created the first buildable macOS shell project under `platforms/macos/app/`
- connected it to shared workspace and diagnostics libraries
- introduced a separate platform-local macOS solution for future work

## 2026-04-18 15:17:29 +09

Reason for change:

- fix a Windows UI crash when opening `Settings` from the operator screen
- restore a working operator package after a localization format-string mismatch in `message.settings_locked_info`
- refresh the external Windows runtime and release zip with the rebuilt `AgentBrowserUI.exe`

Files edited:

- `AgentBrowserUi/Program.cs`
- `DEVLOG.md`

Package notes:

- rebuilt `AgentBrowserUI.exe` with `dotnet build AgentBrowserUi/AgentBrowserUi.csproj -c Release`
- published `AgentBrowserUI.exe` with `dotnet publish AgentBrowserUi/AgentBrowserUi.csproj -c Release -r win-x64 --self-contained true`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/AgentBrowserUI.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/releases/AgentBrowser_Windows.zip`

Change summary:

- fixed the missing format argument by passing `Environment.NewLine` into the locked-settings message
- hardened `UiText.Format` so a future placeholder mismatch no longer crashes the entire UI
- rebuilt and repackaged the Windows operator bundle on external storage

## 2026-04-18 15:17:29 +09

Reason for change:

- simplify the VPN admin flow after feedback that `Settings` had become too hard to reach
- make the operator/admin lock optional instead of forcing an unlock mechanism during normal preset editing
- update the packaged manuals to match the new default behavior

Files edited:

- `AgentBrowserUi/Program.cs`
- `ADR/ADR-0006-operator-main-screen-and-locked-settings.md`
- `AgentBrowser_Windows/README.txt`
- `AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- `DEVLOG.md`

Package notes:

- rebuilt `AgentBrowserUI.exe` with `dotnet build AgentBrowserUi/AgentBrowserUi.csproj -c Release`
- published `AgentBrowserUI.exe` with `dotnet restore AgentBrowserUi/AgentBrowserUi.csproj -r win-x64` and `dotnet publish AgentBrowserUi/AgentBrowserUi.csproj -c Release -r win-x64 --self-contained true --no-restore`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/AgentBrowserUI.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/README.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/PRO_USER_MANUAL.txt`

Change summary:

- changed `Settings` access so it opens directly when no admin password is configured
- kept password prompt behavior only for explicitly locked packages using `AGENT_BROWSER_PRO_PASSWORD`
- retained `pro-mode.flag` as a local bypass for admin access

## 2026-04-18 15:17:29 +09

Reason for change:

- clarify a Windows operator failure where `Start.exe` launch was canceled in the administrator prompt
- replace the raw system exception text with a specific UAC/admin-rights message
- update operator-facing docs so non-admin users understand why the session does not start

Files edited:

- `AgentBrowserUi/Program.cs`
- `AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- `AgentBrowser_Windows/README.txt`
- `DEVLOG.md`

Package notes:

- rebuilt `AgentBrowserUI.exe` with `dotnet build AgentBrowserUi/AgentBrowserUi.csproj -c Release`
- published `AgentBrowserUI.exe` with `dotnet publish AgentBrowserUi/AgentBrowserUi.csproj -c Release -r win-x64 --self-contained true`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/AgentBrowserUI.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/README.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/BASIC_USER_MANUAL.txt`

Change summary:

- detect Windows `ERROR_CANCELLED` when launching helper executables from the UI
- show a clear admin/UAC explanation instead of the raw process-start exception
- documented that `Start.exe` requires confirming the Windows elevation prompt or entering local admin credentials

## 2026-04-18 20:07:51 +09

Reason for change:

- document the observed real-world fallback flow where `Start.exe` is launched manually while `AgentBrowserUI.exe` remains open
- capture the practical instruction that manual elevation of `Start.exe` still updates the open UI correctly
- align operator and admin manuals with the tested Windows behavior

Files edited:

- `AgentBrowser_Windows/README.txt`
- `AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- `AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- `DEVLOG.md`

Package notes:

- no source code changed
- no binaries were rebuilt
- refreshed package manuals in `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/`
- refreshed `/Volumes/Yablocko/AgentBrowser/releases/AgentBrowser_Windows.zip`

Change summary:

- added a documented fallback method: keep UI open, launch `Start.exe` manually, approve UAC, and watch the UI reflect the running session
- clarified that this is a supported practical recovery path when button-driven elevation is awkward on a given Windows machine

## 2026-04-18 23:01:35 +0900

Reason for change:

- switch the default Windows runtime from machine-wide `System TUN` to `Browser-only Proxy`
- keep `System TUN` as an explicit advanced mode with conditional elevation only for that path
- finish the external package refresh so `/Volumes/Yablocko` no longer ships the stale TUN-first runtime state

Files edited:

- `libs/AgentBrowser.Config/RuntimeMode.cs`
- `libs/AgentBrowser.Config/SingBoxConfigBuilder.cs`
- `libs/AgentBrowser.Sessions/SessionRuntimeHelpers.cs`
- `libs/AgentBrowser.Sessions/StartupFlow.cs`
- `SingBoxStart/app.manifest`
- `SingBoxStop/app.manifest`
- `AgentBrowserUi/Program.cs`
- `README.md`
- `ARCHITECTURE.md`
- `CONFIGURATION.md`
- `STATUS.md`
- `DEVLOG.md`
- `AgentBrowser_Windows/README.txt`
- `AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- `AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- `ADR/ADR-0011-browser-only-proxy-default-with-optional-system-tun.md`

Package notes:

- `dotnet build AgentBrowser.sln -c Release` passed successfully
- published `AgentBrowserUI.exe`, `Start.exe`, `Stop.exe`, and `SupportTool.exe` for `win-x64`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/AgentBrowserUI.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/Start.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/Stop.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/SupportTool.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/README.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/ui-settings.json`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/workspaces/default/ui-settings.json`
- regenerated `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/core/config.json` in `BrowserProxy` mode from the current builder
- rebuilt `/Volumes/Yablocko/AgentBrowser/releases/AgentBrowser_Windows.zip`

Change summary:

- introduced a shared `RuntimeMode` model so presets, config generation, UI, and startup flow all agree on the selected runtime path
- made `Browser-only Proxy` the default and recommended operator mode, launching the bundled browser with `--proxy-server=socks5://127.0.0.1:1080`
- kept `System TUN` as an advanced path and requested elevation with `runas` only when that explicit mode is selected
- changed `Start.exe` and `Stop.exe` manifests to `asInvoker`, with `wintun.dll` validation only in `System TUN`
- refreshed the external runtime and release archive so manual `Start.exe` no longer revives the stale TUN-first package state

## 2026-06-03 09:41:34 +0900

Reason for change:

- prevent the UI/start-stop flow from crashing when `core/config.json` is missing, malformed, wrong-shaped, or temporarily unreadable
- keep `Browser-only Proxy` as the safe fallback runtime mode
- add focused automated coverage for runtime-mode inference

Files edited:

- `AgentBrowser.sln`
- `libs/AgentBrowser.Config/SingBoxConfigBuilder.cs`
- `tests/AgentBrowser.Config.Tests/AgentBrowser.Config.Tests.csproj`
- `tests/AgentBrowser.Config.Tests/SingBoxConfigBuilderRuntimeModeTests.cs`
- `DEVLOG.md`

Package notes:

- no package artifacts were refreshed
- no publish/deploy was run
- no external runtime or release zip was modified
- `dotnet build AgentBrowser.sln`, `dotnet test AgentBrowser.sln`, and `dotnet build AgentBrowser.sln -c Release` passed

Change summary:

- added a fail-safe runtime-mode inference guard around config file read/parse failures
- malformed or wrong-shaped `core/config.json` now falls back to `BrowserProxy` instead of throwing
- added tests for missing config, SOCKS inbound, TUN inbound, malformed config, non-array `inbounds`, and non-string inbound `type`

## 2026-06-10 10:12:28 +0900

Reason for change:

- fix PR #1 review findings before any package refresh/deploy gate
- keep the shared direct connectivity probe client alive across repeated `System TUN` probes
- preserve existing `System TUN` packages when loading old preset settings that do not yet contain `RuntimeMode`

Files edited:

- `AgentBrowserUi/AgentBrowserUi.csproj`
- `AgentBrowserUi/Program.cs`
- `libs/AgentBrowser.Config/SettingsRuntimeModeMigration.cs`
- `libs/AgentBrowser.Sessions/ConnectivityHttpClientLease.cs`
- `tests/AgentBrowser.Config.Tests/AgentBrowser.Config.Tests.csproj`
- `tests/AgentBrowser.Config.Tests/ConnectivityHttpClientLeaseTests.cs`
- `tests/AgentBrowser.Config.Tests/SettingsRuntimeModeMigrationTests.cs`
- `DEVLOG.md`

Package notes:

- no package artifacts were refreshed
- no publish/deploy was run
- no external runtime or release zip was modified
- `dotnet build AgentBrowser.sln`, `dotnet test AgentBrowser.sln`, and `dotnet build AgentBrowser.sln -c Release` passed

Change summary:

- replaced the connectivity probe's conditional `using HttpClient` with a runtime-aware lease that only disposes temporary Browser Proxy clients
- added settings JSON migration for old presets missing `RuntimeMode`, using the current `core/config.json` runtime mode as the fallback
- made settings deserialization case-insensitive and enum-string aware so old and current settings shapes can be loaded safely
- added regression tests for both PR #1 findings

## 2026-06-10 11:52:47 +0900

Reason for change:

- resolve the package refresh preflight finding where external package docs mentioned HTTP while current source supports only SOCKS/VLESS
- keep source code as the source of truth for connection types before any package refresh
- make `Start.exe` and `Stop.exe` publish as standalone single-file executables, matching the current external runtime copy model

Files edited:

- `SingBoxStart/Start.csproj`
- `SingBoxStop/Stop.csproj`
- `DEVLOG.md`

Package notes:

- no package artifacts were refreshed
- no external runtime or release zip was modified
- Windows manual smoke remains unverified because no Windows machine is available
- `dotnet build AgentBrowser.sln`, `dotnet test AgentBrowser.sln`, and `dotnet build AgentBrowser.sln -c Release` passed
- `dotnet publish SingBoxStart/Start.csproj -c Release -r win-x64 --self-contained true -o .publish-check/Start` passed
- `dotnet publish SingBoxStop/Stop.csproj -c Release -r win-x64 --self-contained true -o .publish-check/Stop` passed

Change summary:

- added `PublishSingleFile` to the `Start` and `Stop` Windows helper projects
- verified scratch publish output contains standalone `Start.exe` / `Stop.exe` plus PDBs, with no adjacent DLLs required for those helpers
- confirmed current source/docs do not contain HTTP connection support, so HTTP should not be copied from the external package manuals into a refreshed package unless a separate HTTP-support change is made first

## 2026-06-10 14:48:01 +0900

Reason for change:

- refresh the external Windows runtime and release zip after PR #2 merged
- align shipped package manuals with current source docs and remove the stale external HTTP wording
- keep existing sensitive runtime settings while updating binaries and package docs

Files edited:

- `DEVLOG.md`

Package notes:

- backup created at `/Volumes/Yablocko/AgentBrowser/backups/20260610-144147`
- fresh publish output created at `.publish-refresh/20260610-144321`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/AgentBrowserUI.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/Start.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/Stop.exe`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/SupportTool.exe`
- refreshed WPF runtime DLLs in `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/README.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/BASIC_USER_MANUAL.txt`
- refreshed `/Volumes/Yablocko/AgentBrowser/runtime/AgentBrowser_Windows/PRO_USER_MANUAL.txt`
- rebuilt `/Volumes/Yablocko/AgentBrowser/releases/AgentBrowser_Windows.zip`
- kept `ui-settings.json`, `workspaces/default/ui-settings.json`, and `core/config.json` unchanged
- kept `core/sing-box.exe`, `core/wintun.dll`, and `core/libcronet.dll` unchanged
- Windows manual smoke remains unverified because no Windows machine is available

Validation:

- external manuals now match repo package manuals
- external manuals no longer contain HTTP wording
- `zip_junk=absent`
- zip entries: `1512`
- `unzip -tqq /Volumes/Yablocko/AgentBrowser/releases/AgentBrowser_Windows.zip` passed
- zip SHA256: `24bf57f1614c0cbc8be91f515657e81f596989f2e46c8aa4b800a48c159b737d`
- `AgentBrowserUI.exe` SHA256: `a601020eaa777fc07319d478993b09910889a18a0e7cbf952c52686d77214f10`
- `Start.exe` SHA256: `af7739ff29f51324b4a500800904516dec7dbeb9ef4546c7a9990e1f79a72846`
- `Stop.exe` SHA256: `6cd0bfaf9a6be3638617b8783d208d9e1ae0301ead70b855af6d909230faccee`
- `SupportTool.exe` SHA256: `8b2eb043bb9a07c67731adbaddf4e2b23359899617f0a22f850a84912803ba52`

Change summary:

- refreshed only approved package artifacts on external storage
- preserved existing sensitive configs and core network runtime files
- rebuilt the distributable zip from the refreshed runtime folder

## 2026-06-10 15:10:09 +0900

Reason for change:

- prepare a repeatable Windows smoke handoff after the refreshed package passed non-Windows integrity checks
- make the Windows tester instructions explicit without exposing sensitive runtime configs
- register the smoke checklist as part of the durable documentation set

Files edited:

- `WINDOWS_SMOKE_CHECKLIST.md`
- `DOCUMENT_REGISTRY.md`
- `SOURCES_OF_TRUTH.md`
- `README.md`
- `DEVLOG.md`

Package notes:

- no package artifacts were refreshed in this change
- no external runtime or release zip was modified in this change
- Windows manual smoke remains pending until a Windows tester runs the checklist

Validation:

- checklist includes artifact hash, required smoke steps, optional System TUN path, pass criteria, and report template
- checklist instructs testers not to send raw secrets or full config contents

Change summary:

- added a formal handoff document for Windows smoke verification
- linked the document from the docs index, registry, and sources of truth
