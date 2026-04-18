# Decision Log

## Purpose

This file is the index of formal architecture and product decisions.

Use it to answer:

- which decisions are currently accepted
- where the full rationale lives
- whether a decision has been superseded

Detailed decisions live in `ADR/`.

## Status Values

- `Accepted`: current decision
- `Superseded`: replaced by a later decision
- `Draft`: proposed but not yet adopted

## Decision Index

| ADR | Title | Status | Date | Scope |
| --- | --- | --- | --- | --- |
| `ADR-0001` | Managed Browser Workspace Positioning | Accepted | 2026-04-18 | product framing |
| `ADR-0002` | `sing-box` TUN Runtime As The Active Network Architecture | Accepted | 2026-04-18 | runtime architecture |
| `ADR-0003` | Documentation Governance Via Registry, Source Of Truth, And DevLog | Accepted | 2026-04-18 | documentation governance |
| `ADR-0004` | Basic And Pro Operator Modes In The UI | Superseded | 2026-04-18 | operator UX |
| `ADR-0005` | Release And Change Checklist Enforcement | Accepted | 2026-04-18 | release governance |
| `ADR-0006` | Operator Main Screen And Locked Admin Settings | Accepted | 2026-04-18 | operator/admin UX |
| `ADR-0007` | Source Repository Excludes Runtime Artifacts | Accepted | 2026-04-18 | repository boundary |
| `ADR-0008` | Single Source Repository For Windows And Future macOS | Accepted | 2026-04-18 | platform repository scope |
| `ADR-0009` | Platform-Specific Source Lives Under `platforms/<platform>` | Accepted | 2026-04-18 | platform layout |

## Rules

1. Every durable architecture or product-direction decision should get an ADR.
2. `DECISION_LOG.md` is the entry point; the ADR file contains the full rationale.
3. If a decision changes, do not silently rewrite history. Add a new ADR and mark the older one superseded if needed.
4. After adding or changing an ADR, update:
   - `DECISION_LOG.md`
   - `DOCUMENT_REGISTRY.md`
   - `DEVLOG.md`
