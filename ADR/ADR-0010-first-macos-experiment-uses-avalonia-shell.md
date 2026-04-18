# ADR-0010: First macOS Experiment Uses An Avalonia Desktop Shell

## Status

Accepted

## Date

2026-04-18

## Context

The repository now has:

- one source repository for Windows now and macOS later
- a platform skeleton under `platforms/macos/`
- shared `.NET` libraries in `libs/`

The next step is to choose the first real macOS experiment.

The experiment should reduce risk, not maximize ambition. The highest-value unknown is whether the product can host a real macOS desktop shell while still reusing the existing shared `.NET` layers.

## Decision

The first macOS experiment will use:

- an `Avalonia` desktop shell under `platforms/macos/app/`
- shared `.NET` libraries where reuse is practical

The first experiment will not include:

- `TUN` integration
- full tunnel orchestration
- privilege or entitlement work
- production packaging/signing/notarization

This is a shell-first experiment, not a parity-first experiment.

## Consequences

Positive:

- fastest path to a real macOS UI experiment
- highest reuse of shared `.NET` code
- avoids overcommitting to platform-specific networking before the shell exists

Tradeoffs:

- it does not prove final macOS runtime feasibility by itself
- a later decision will still be needed for tunnel strategy and privilege model
- if the shell proves unsatisfactory, the project may still pivot to a native macOS UI stack later

## Follow-Up

- create the first `Avalonia`-based macOS shell project inside `platforms/macos/app/`
- keep runtime and packaging work separate until the shell exists
- revisit stack choice only if the shell experiment fails technically or product-wise
