# ADR-0006: Operator Main Screen And Locked Admin Settings

- Status: Accepted
- Date: 2026-04-18

## Context

The first MVP split the UI into `Basic` and `Pro` inside the same window.

That technically hid secret connection fields, but it still left several admin concepts visible on the operator screen:

- a mode switch
- preset management controls
- an unclear password-based unlock path

For handoff scenarios, the package needs a stricter operator surface:

- normal users should see only run-time controls
- admins should have a separate place for editing presets and connection strings

## Decision

Adopt a two-surface UI model:

- the main window is the operator screen
- admin editing is moved into a separate `Settings` window

Behavior:

- the main screen shows only the current preset, masked summary, runtime state, connection health, and main actions
- preset editing, connection editing, `Test`, `Apply`, `Duplicate`, `Rename`, and `Delete` live in `Settings`
- `Settings` is gated by `pro-mode.flag` or `AGENT_BROWSER_PRO_PASSWORD`
- visible UI text follows the system language heuristic for `ru` and `en`

## Consequences

Positive:

- clearer operator/admin boundary
- less accidental exposure of proxy or VLESS details
- simpler handoff model for preconfigured packages

Negative:

- one more dialog to maintain
- selected preset changes now happen through the settings flow instead of the operator screen
- localization needs continuous maintenance as UI text changes
