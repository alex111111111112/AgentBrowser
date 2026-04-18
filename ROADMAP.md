# Roadmap

## Near Term

- stabilize `VLESS` runtime path on Windows with real-world validation
- improve UI feedback when `Start.exe` fails after UAC or when helper launch is blocked
- surface `sing-box` runtime errors inside the UI instead of requiring manual log inspection
- reduce ambiguity between generated config success and actual connectivity success
- make packaged builds reproducible and easy to refresh without manual file drift
- define product language around managed browser and session isolation
- split current browser state into portable and local-only categories
- create a workspace manifest and session-status model
- execute the first extraction batch from `WORKSPACE_V2_BACKLOG.md`

## Medium Term

- add explicit preset management actions: rename, duplicate, delete
- add connection test that checks actual outbound reachability, not only config syntax
- add export and import for presets
- add clearer runtime status for both browser and tunnel separately
- add packaged version metadata in UI and logs
- add support bundle export for support and customer issue reports
- add profile templates and locked operator policies
- document what profile data is portable and what authentication state is not
- add workspace export/import flow
- add explicit re-auth checklist after workspace import
- add per-device workspace identity
- define macOS platform boundary and shared-vs-platform-specific module split before starting implementation

## Long Term

- split packaged runtime artifacts from source repository more cleanly
- automate packaging into a reproducible build script
- add a dedicated `docs/` section or generated docs site if the project grows
- evaluate whether `Start.exe` and `Stop.exe` should become a single service-style helper with IPC
- add licensing, update delivery, and release channels for commercial distribution
- add a real session portability model if cross-machine auth transfer is required
- add optional control plane for workspace sync and tenant management
- add a macOS runtime track in the same repository with explicit packaging and support boundaries

## Explicit Non-Goals For Now

- Linux runtime support
- multi-user profile management
- full anti-detect browser behavior
- broad protocol support beyond current `SOCKS5` and `VLESS TCP`
- hardware fingerprint spoofing
- claims of invisibility or “undetectable” browsing

macOS is not a delivered runtime in the current milestone, but it is no longer treated as a repository-level non-goal.
