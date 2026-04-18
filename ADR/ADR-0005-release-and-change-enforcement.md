# ADR-0005: Release And Change Checklist Enforcement

- Status: Accepted
- Date: 2026-04-18

## Context

The project already has:

- a document registry
- a source-of-truth map
- a decision log and ADRs
- a required devlog discipline

Without explicit completion gates, maintainers can still skip the last operational step and call a change or release done too early.

## Decision

Introduce:

- `CHANGE_CHECKLIST.md` for non-trivial changes
- `RELEASE_CHECKLIST.md` for package refreshes and release candidates
- `PULL_REQUEST_TEMPLATE.md` for structured change summaries

Contributor rule:

- a change is not complete until the relevant checklist is satisfied and `DEVLOG.md` is updated
- a package refresh is not complete until the release checklist is satisfied

## Consequences

Positive:

- clearer release discipline
- lower chance of undocumented package refreshes
- stronger consistency between code, docs, and package outputs

Negative:

- more process overhead on each substantial change
- contributors must decide whether a change is trivial or non-trivial

