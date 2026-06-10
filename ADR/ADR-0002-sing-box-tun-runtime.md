# ADR-0002: `sing-box` TUN Runtime As The Active Network Architecture

- Status: Superseded
- Date: 2026-04-18

Superseded by:

- `ADR-0011-browser-only-proxy-default-with-optional-system-tun.md`

## Context

Earlier iterations used Xray launchers and explored other client models such as `v2rayN`.

Those approaches were less suitable for the desired portable operator flow because of:

- packaging brittleness
- GUI/runtime coupling
- weaker fit for a controlled one-click browser workspace

The current implementation already runs through `sing-box` in `TUN` mode with admin elevation and path-scoped process control.

## Decision

Treat `sing-box` in `TUN` mode as the active network runtime architecture for the current product line.

Operational path:

- `AgentBrowserUI.exe` writes config
- `Start.exe` validates and starts `sing-box`
- bundled browser runs behind the tunnel
- `Stop.exe` stops only package-owned browser and tunnel processes

Legacy Xray artifacts remain reference-only unless an explicit rollback or historical task requires them.

## Consequences

Positive:

- consistent operator workflow
- portable runtime with fewer GUI dependencies
- unified diagnostics path around `sing-box`

Negative:

- `TUN` requires admin rights
- Windows-local networking issues can still break the session even if config validation passes
