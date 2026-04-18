# Workspace V2 Backlog

## Goal

Move the current project from:

- one portable package
- one implicit browser profile
- one generated tunnel config

to:

- explicit workspaces
- explicit device-local vs portable state
- export/import flow
- re-auth aware UX

without breaking the current `AgentBrowser_Windows` package during migration.

## Constraint

The current shipping flow must keep working while the refactor happens:

- `AgentBrowserUI.exe`
- `Start.exe`
- `Stop.exe`
- `AgentBrowser_Windows/`

That means `v2` must be introduced in slices, not as a rewrite.

## Current To Target Mapping

Current:

- `AgentBrowserUi/`
- `SingBoxStart/`
- `SingBoxStop/`
- `AgentBrowser_Windows/core/config.json`
- `AgentBrowser_Windows/ui-settings.json`
- `AgentBrowser_Windows/Profile/`

Target:

- UI owns workspace selection and operator actions
- launcher owns runtime orchestration only
- config generation moves into a reusable library
- profile data is split into portable and local-only buckets
- session state becomes explicit metadata instead of implicit browser files

## Recommended Repository Layout

Keep current projects for now, but evolve toward this:

```text
apps/
├── AgentBrowser.Ui/
├── AgentBrowser.Start/
└── AgentBrowser.Stop/

libs/
├── AgentBrowser.Workspaces/
├── AgentBrowser.Config/
├── AgentBrowser.Profiles/
├── AgentBrowser.Sessions/
├── AgentBrowser.Diagnostics/
├── AgentBrowser.Export/
└── AgentBrowser.Devices/

packaging/
└── windows/

runtime/
└── templates/
```

For the current repository, this can be approximated incrementally by first creating:

- `libs/AgentBrowser.Workspaces/`
- `libs/AgentBrowser.Config/`
- `libs/AgentBrowser.Profiles/`
- `libs/AgentBrowser.Sessions/`
- `libs/AgentBrowser.Diagnostics/`

and then updating existing app projects to reference them.

## First Refactor Rule

Do not start by moving packaged files around.

Start by extracting logic from `Program.cs` files into reusable classes while keeping:

- executable names unchanged
- package layout unchanged
- generated outputs unchanged

## Module Plan

## `AgentBrowser.Workspaces`

Purpose:

- create, load, rename, clone, and select workspaces
- own manifest persistence
- define workspace paths

Initial classes:

- `WorkspaceId`
- `WorkspaceManifest`
- `WorkspacePaths`
- `WorkspaceService`
- `WorkspaceRepository`

Suggested files:

```text
libs/AgentBrowser.Workspaces/
├── WorkspaceId.cs
├── WorkspaceManifest.cs
├── WorkspacePaths.cs
├── WorkspaceService.cs
└── WorkspaceRepository.cs
```

## `AgentBrowser.Config`

Purpose:

- parse SOCKS and VLESS connection strings
- generate `sing-box` config
- validate config request models before writing JSON

Extract from current UI:

- SOCKS parser
- VLESS parser
- `BuildBaseConfig`
- `BuildSocksConfig`
- `BuildVlessConfig`

Initial classes:

- `ConnectionKind`
- `SocksConnection`
- `VlessConnection`
- `ConnectionParser`
- `SingBoxConfigBuilder`
- `ConfigValidationResult`

Suggested files:

```text
libs/AgentBrowser.Config/
├── ConnectionKind.cs
├── SocksConnection.cs
├── VlessConnection.cs
├── ConnectionParser.cs
├── SingBoxConfigBuilder.cs
└── ConfigValidationResult.cs
```

## `AgentBrowser.Profiles`

Purpose:

- define which browser data is portable
- define which browser data is device-local
- prepare future export/import rules

Initial classes:

- `ProfilePaths`
- `ProfileClassifier`
- `PortableProfileService`
- `LocalProfileService`
- `ProfileSnapshotPlan`

This module should start as metadata and path planning only. Do not begin by copying browser databases blindly.

## `AgentBrowser.Sessions`

Purpose:

- describe auth portability limits explicitly
- track per-provider re-auth status
- track workspace-to-device session state

Initial classes:

- `SessionStatus`
- `ProviderSessionState`
- `SessionManifest`
- `SessionManifestService`

Suggested principle:

This module stores what the app knows about auth state. It does not store or migrate raw browser secrets in `v1`.

## `AgentBrowser.Devices`

Purpose:

- identify the current device
- support future multi-device workspace behavior

Initial classes:

- `DeviceId`
- `DeviceInfo`
- `DeviceIdentityService`

`DeviceIdentityService` can start with a generated local device ID persisted to disk.

## `AgentBrowser.Diagnostics`

Purpose:

- unify logging behavior across UI, Start, and Stop
- create support bundles later

Extract from current apps:

- `UiLog`
- start logger
- stop logger

Initial classes:

- `LogWriter`
- `LogPaths`
- `SupportBundleService`

## App Refactor Plan

## `AgentBrowserUI.exe`

Current role:

- presets
- config generation
- test
- helper launch

Target role:

- workspace selection
- preset editor
- session status display
- export/import actions
- helper launch

What to move out first:

- all parser and config-builder logic
- settings file loading/saving

What stays in UI:

- forms
- button handlers
- display state

## `Start.exe`

Current role:

- validate files
- validate config
- launch tunnel
- launch browser
- wait for browser exit
- stop tunnel

Target role:

- resolve selected workspace
- resolve runtime paths from workspace
- start session from explicit launch context

What to move out first:

- file validation helpers
- browser path resolution
- process counting/path matching
- launch context creation

Suggested classes:

- `LaunchContext`
- `RuntimePathResolver`
- `BrowserProcessLocator`
- `TunnelProcessLocator`
- `SessionRunner`

## `Stop.exe`

Current role:

- kill bundled browser
- kill tunnel

Target role:

- stop current workspace session
- optionally stop by workspace ID later

What to move out first:

- process path matching
- kill/wait logic

Suggested classes:

- `StopContext`
- `ProcessPathMatcher`
- `WorkspaceStopService`

## Packaging Strategy

Do not change the outer package path in the first migration stage.

Instead, introduce this inside the existing package:

```text
AgentBrowser_Windows/
├── workspaces/
│   └── default/
│       ├── manifest.json
│       ├── ui-settings.json
│       ├── session-status.json
│       ├── profile-portable/
│       └── profile-local/
├── core/
├── App/
├── Data/
└── Profile/
```

Migration note:

At first, `Profile/` can remain the actual live browser profile, while `workspaces/default/` is introduced as metadata only.

This avoids breaking the current launcher.

## Migration Stages

## Stage 0: No Behavior Change

Deliverables:

- new libraries created
- logic extracted from UI and helper exes
- current package still works exactly the same

Success condition:

- same package layout
- same executable names
- same runtime behavior

## Stage 1: Workspace Metadata

Deliverables:

- `workspaces/default/manifest.json`
- `workspaces/default/ui-settings.json`
- `workspaces/default/session-status.json`
- UI loads workspace metadata from explicit workspace folder

Success condition:

- current single-workspace flow still works
- no export/import yet

## Stage 2: Portable vs Local Profile Split

Deliverables:

- profile classification rules
- `profile-portable/` and `profile-local/` directories introduced
- documentation for what is intentionally not portable

Success condition:

- workspace structure is explicit
- no false promise about auth portability

## Stage 3: Export / Import

Deliverables:

- workspace export command
- workspace import command
- re-auth checklist generation

Success condition:

- operators can move a workspace
- product clearly identifies expected re-login requirements

## Stage 4: Device Awareness

Deliverables:

- local `device-id`
- per-device session status
- better UI around “last authenticated on device X”

Success condition:

- multi-device behavior is explainable
- support can reason about session portability failures

## Stage 5: Commercial Layer

Deliverables:

- license checks
- update delivery
- optional cloud sync
- support bundle upload

Success condition:

- product can be operated commercially without changing the workspace model again

## First Concrete Backlog

If starting implementation now, do this in order.

1. Create `libs/AgentBrowser.Config/` and move all connection parsing plus `sing-box` JSON generation there.
2. Create `libs/AgentBrowser.Diagnostics/` and unify logging helpers used by UI, Start, and Stop.
3. Create `libs/AgentBrowser.Workspaces/` with `WorkspaceManifest`, `WorkspacePaths`, and `WorkspaceService`.
4. Make UI read and write `workspaces/default/ui-settings.json` while still mirroring to the current root location if needed.
5. Add `workspaces/default/manifest.json` generation.
6. Introduce `session-status.json` as metadata only, without trying to move browser secrets.
7. Refactor `Start.exe` to build a `LaunchContext` from workspace paths instead of hard-coded root paths.
8. Refactor `Stop.exe` to stop the current workspace session from a `StopContext`.
9. Only after that, design export/import.

## Tickets To Open

Suggested first ticket set:

- `CFG-001` Extract connection parsing from UI
- `CFG-002` Extract sing-box config builder
- `LOG-001` Introduce shared logging utility
- `WS-001` Add workspace manifest model
- `WS-002` Add workspace paths resolver
- `WS-003` Add default workspace bootstrap
- `SES-001` Add session-status metadata model
- `APP-UI-001` Switch UI settings path to workspace-aware resolution
- `APP-START-001` Introduce LaunchContext
- `APP-STOP-001` Introduce StopContext

## Decision Rule

If a proposed change forces a package-layout break before workspace metadata exists, it is probably too early.

The migration should preserve:

- current package usability
- current logs
- current executable names
- current support workflow

until `Stage 3` is finished.
