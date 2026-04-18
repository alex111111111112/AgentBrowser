# ADR-0003: Documentation Governance Via Registry, Source Of Truth, And DevLog

- Status: Accepted
- Date: 2026-04-18

## Context

The project has grown from a single launcher task into a bundle of runtime code, package artifacts, user manuals, planning docs, and operational guides.

Without explicit governance, maintainers will eventually hit these problems:

- duplicate docs with conflicting guidance
- unclear ownership of truth for a domain
- undocumented architectural drift

## Decision

Use the following governance model:

- `DOCUMENT_REGISTRY.md` defines which project docs are official and active
- `SOURCES_OF_TRUTH.md` defines the authoritative source per domain
- `DEVLOG.md` records every source or package-facing change
- `DECISION_LOG.md` and `ADR/` capture durable architecture and product decisions

## Consequences

Positive:

- clearer maintenance discipline
- easier conflict resolution
- explicit historical record for design choices

Negative:

- more documentation overhead on each substantial change
- contributors must update multiple governance files when structure changes

