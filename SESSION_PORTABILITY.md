# Session Portability V2

## Why This Exists

The current package is portable at the file level, but browser authentication is not reliably portable across Windows machines.

That is not a packaging bug. It is an architectural boundary:

- browser settings and profile files are copyable
- sensitive auth material is often protected by Windows-bound encryption
- copying the raw profile directory is therefore not a real cross-device session strategy

This document defines a safer and more supportable `v2` architecture.

## Product Goal

Support moving a managed browser workspace between devices without pretending that raw browser secrets are universally portable.

Desired outcome:

- the same workspace opens on another machine
- tunnel presets and operator settings survive
- browser shape and profile behavior survive
- the product clearly tells the operator which services must be re-authenticated

## Non-Goals

This design does not aim to:

- bypass Windows-bound secret protection
- copy raw encrypted cookies and promise they will work elsewhere
- claim seamless auth portability for all websites
- implement stealth or anti-detect behavior

## Design Principle

Split data into three classes.

## Class A: Fully Portable Workspace Data

This data should sync or export cleanly:

- preset definitions
- tunnel settings
- UI settings
- bookmarks
- extension allowlist
- homepage/new-tab/startup preferences
- browser launch policy
- profile metadata
- support metadata and logs

## Class B: Conditionally Portable Browser State

This data may be portable in some cases, but should not be treated as guaranteed:

- non-sensitive profile caches
- some local storage
- some extension state
- non-secret browsing history

This data can be included in export or sync behind policy, but the product must tolerate loss.

## Class C: Non-Portable Device-Bound Secrets

This data should be treated as local-only by default:

- cookies used for authenticated sessions
- saved passwords
- refresh tokens
- browser encryption keys
- secrets protected by DPAPI or other Windows-bound mechanisms

The system should never promise portability for this class.

## Recommended V2 Architecture

```text
apps/
├── ui/
├── launcher/
└── stopper/

libs/
├── workspace-core/
├── profile-layout/
├── config-engine/
├── session-manifest/
├── diagnostics/
├── export-import/
└── secret-boundary/

services/
└── optional-control-plane/
```

## Local Filesystem Layout

```text
AgentBrowser/
├── App/
├── core/
├── runtime/
├── workspaces/
│   └── {workspace-id}/
│       ├── manifest.json
│       ├── ui-settings.json
│       ├── browser-policy.json
│       ├── bookmarks.json
│       ├── extensions.json
│       ├── profile-portable/
│       ├── profile-local/
│       ├── export-cache/
│       └── logs/
├── devices/
│   └── {device-id}/
│       ├── device.json
│       └── secret-state.json
└── package-metadata/
```

## Core Modules

### `workspace-core`

Owns:

- workspace IDs
- workspace manifest
- current workspace selection
- lifecycle operations: create, clone, export, import

### `profile-layout`

Owns the split between:

- `profile-portable/`
- `profile-local/`

`profile-portable/` should contain syncable state.

`profile-local/` should contain machine-bound or riskier runtime residue.

### `session-manifest`

Tracks what the product knows about login state without trying to copy raw secrets.

Example responsibility:

- “Google requires re-login”
- “ChatGPT session last known valid on device A”
- “Cookie portability not guaranteed”

This is a metadata layer, not a secret export layer.

### `export-import`

Builds portable workspace bundles.

Should export:

- workspace manifest
- presets
- bookmarks
- portable profile subset
- operator-facing re-auth checklist

Should not export by default:

- raw encrypted browser auth databases
- device-bound secrets

### `secret-boundary`

Defines explicit rules for what is local-only.

If later you add OS-specific secret storage, that module should own:

- Windows Credential Manager integration
- DPAPI-bound local secrets
- migration rules
- secure wipe rules

### `optional-control-plane`

Commercial layer for:

- license management
- workspace sync
- tenant management
- update channels
- support bundle upload

This should stay optional in the architecture so local-only builds still work.

## Sync Model

Recommended sync model:

1. Sync workspace metadata and portable profile subset
2. Restore browser shape on another machine
3. Detect missing or invalid local auth state
4. Ask operator to re-authenticate only the providers that need it
5. Update workspace manifest after successful re-auth

This is more supportable than pretending raw browser sessions are universally movable.

## UX Model

The UI should eventually expose:

- `Workspace`
- `Device`
- `Portable Data`
- `Local Secrets`
- `Export`
- `Import`
- `Re-auth Needed`

This lets the product explain reality instead of hiding it.

## Suggested Data Contracts

### `manifest.json`

Contains:

- workspace ID
- display name
- created timestamp
- current browser channel
- current tunnel mode
- sync version

### `session-status.json`

Contains:

- provider ID
- last authenticated device ID
- last success timestamp
- portability status
- re-auth required boolean

### `browser-policy.json`

Contains:

- startup URLs
- extension policy
- launch flags
- network mode

## Migration Path From Current Project

### Stage 1

Refactor current package into:

- workspace metadata
- portable profile subset
- local profile subset

No backend required.

### Stage 2

Add:

- export workspace
- import workspace
- re-auth checklist

Still no promise of auth portability.

### Stage 3

Add device identity:

- local device ID
- per-device workspace status
- per-provider session status

### Stage 4

Add commercial control plane:

- user accounts
- licenses
- workspace sync
- audit events

### Stage 5

Add opinionated enterprise features:

- policy locks
- managed extensions
- update rings
- tenant-level browser policy

## Recommended Product Messaging

Say:

- portable workspace
- portable browser environment
- managed browser session
- profile portability with controlled re-auth

Do not say:

- portable auth everywhere
- guaranteed session cloning
- anti-detect portability
- invisible browser identity

## Engineering Conclusion

If cross-machine auth persistence matters, the right product is not “copy the browser folder better”.

The right product is:

- portable workspace
- explicit secret boundary
- re-auth aware UX
- optional control plane for sync and licensing
