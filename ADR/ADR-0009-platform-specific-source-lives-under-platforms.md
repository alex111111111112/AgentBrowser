# ADR-0009: Platform-Specific Source Lives Under `platforms/<platform>`

## Status

Accepted

## Date

2026-04-18

## Context

The repository now explicitly supports one product identity with:

- an active Windows implementation
- shared libraries under `libs/`
- future macOS work in the same repository

Without a clear layout rule, platform-specific work will drift into the root and mix with shared libraries, packaging notes, and legacy artifacts.

The project needs a stable location for future platform-specific implementation work before actual macOS runtime code begins.

## Decision

Platform-specific implementation work will live under:

- `platforms/windows/` if a dedicated Windows source track is later needed
- `platforms/macos/` for future macOS implementation work

Each platform folder should separate concerns into:

- `app/`
- `runtime/`
- `packaging/`
- optional platform-local docs

Cross-platform or reusable logic stays under `libs/`.

Repository-level docs and governance remain in the root and `ADR/`.

This decision creates an empty macOS foundation skeleton now, without committing to a runtime framework or packaging mechanism yet.

## Consequences

Positive:

- platform-specific code has a predictable home
- shared logic is less likely to leak into platform folders and vice versa
- future macOS work can begin without disturbing the current Windows runtime path

Tradeoffs:

- the tree becomes slightly more formal before there is actual macOS code
- maintainers must resist putting shared logic under `platforms/`

## Follow-Up

- keep new shared behavior in `libs/`
- start macOS implementation inside `platforms/macos/` when the first real runtime experiment begins
- revisit the root solution strategy when macOS gains actual source projects
