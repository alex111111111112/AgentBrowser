# ADR Guide

## Purpose

This directory stores Architecture Decision Records.

Each ADR should capture:

- context
- decision
- consequences
- current status

## Naming

Use sequential names:

- `ADR-0001-...`
- `ADR-0002-...`

Keep numbers stable even if titles change later.

Template:

- `ADR/ADR-TEMPLATE.md`

## Status

Use one of:

- `Accepted`
- `Superseded`
- `Draft`

## Update Rule

Do not overwrite an old accepted decision just to match the latest state.

Instead:

1. add a new ADR
2. mark the older ADR superseded if necessary
3. update `DECISION_LOG.md`
4. update `DEVLOG.md`

## When A New ADR Is Required

Create a new ADR when the change affects one or more of these:

- product positioning or market framing
- active runtime architecture
- process ownership or security boundary
- workspace or packaging model
- operator/admin UX model
- documentation governance model
- source-of-truth ownership for a major domain

Typical examples:

- switching the active network runtime
- changing from one UI operating model to another
- introducing or removing a governance layer such as ADRs or Source Of Truth
- redefining what the product is and is not

## When A New ADR Is Not Required

Do not create a new ADR for:

- routine bug fixes
- logging improvements
- packaging refreshes with no architectural change
- copy edits and documentation wording cleanup
- small UX polish that does not change the operating model
- implementation refactors that preserve the same decision

## Decision Threshold

If the answer to this question is "yes", create an ADR:

"Will future maintainers need the reason behind this change, not just the code diff?"
