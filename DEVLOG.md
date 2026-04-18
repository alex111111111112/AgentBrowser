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
