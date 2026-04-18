# ADR-0004: Basic And Pro Operator Modes In The UI

- Status: Superseded by `ADR-0006`
- Date: 2026-04-18

## Context

The same package is used by both:

- operators who only need to run and stop sessions
- admins who need to edit connection settings and manage presets

A single always-technical interface exposes secrets and increases the chance of accidental misconfiguration.

## Decision

Split the UI into:

- `Basic` mode for operators
- `Pro` mode for admins

Behavior:

- `Basic` hides technical inputs and masks saved connection details
- `Pro` exposes connection editing, validation, and preset management
- `Pro` can be gated by `pro-mode.flag` or `AGENT_BROWSER_PRO_PASSWORD`

## Consequences

Positive:

- safer operator UX
- lower chance of accidental edits
- cleaner separation between run-time operation and configuration work

Negative:

- adds one more state to support and test
- requires documented unlock flow for admins

## Superseded Note

This ADR captured the first split between hidden operator fields and visible admin fields inside one window.

It was later replaced by `ADR-0006`, which moves admin editing into a separate Settings window and keeps the main screen operator-only.
