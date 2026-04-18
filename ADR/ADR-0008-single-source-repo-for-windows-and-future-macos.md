# ADR-0008: Single Source Repository For Windows And Future macOS

## Status

Accepted

## Date

2026-04-18

## Context

The project is being pushed to GitHub as a real source repository rather than a local working folder only.

At the moment, the active runtime and packaged deliverable are Windows-first:

- `AgentBrowser_Windows/`
- `Start.exe`
- `Stop.exe`
- `AgentBrowserUI.exe`
- `sing-box` in `TUN` mode on Windows

There is also a likely future need to build a macOS variant of the same managed browser workspace.

The decision point is whether to:

- keep Windows and future macOS work in the same repository
- or split macOS into a separate repository later

## Decision

The project will use a single source repository for:

- the current Windows implementation
- shared product and architecture documents
- shared libraries and orchestration logic
- future macOS implementation work

Windows remains the only active production target right now.

macOS is treated as a planned platform track, not as a currently supported runtime.

Platform-specific runtime packaging should remain separated by clear folders, projects, and packaging instructions instead of mixing artifacts together.

## Consequences

Positive:

- one place for shared docs, ADRs, roadmap, and product direction
- one place for shared libraries and workspace/session logic
- less duplication if macOS reuses parsing, diagnostics, workspace, and support-bundle layers
- easier commercial and product governance because the product identity stays unified

Tradeoffs:

- the repository will eventually contain more platform-specific code
- maintainers must keep platform boundaries explicit
- README and roadmap must state clearly that Windows is active and macOS is planned, not delivered

## Follow-Up

- keep Windows as the default active build path until a real macOS runtime exists
- add macOS work as a separate platform track, not as ad hoc files in the root
- revisit solution and packaging structure when macOS implementation actually starts
