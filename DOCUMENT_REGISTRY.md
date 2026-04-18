# Document Registry

## Purpose

This file is the registry of active human-authored project documents.

Use it to answer:

- which documents are official
- which documents are active vs supporting
- who should read which document
- when a document must be updated

If a new durable document is added and meant to stay in the project, register it here.

## Status Model

- `Active`: current document, part of normal project operations
- `Supporting`: valid, but read only when its topic is needed
- `Package`: shipped with the Windows runtime for end users
- `Generated`: not a governing source, usually build output or third-party text

## Registry

| Document | Status | Primary audience | Purpose | Update trigger |
| --- | --- | --- | --- | --- |
| `README.md` | Active | maintainers, new readers | repository entry point, product framing, docs index | when product framing, build flow, or docs index changes |
| `REPO_SETUP.md` | Active | maintainers, release owner | authoritative repository boundary and GitHub preparation guide | when tracked vs ignored repository scope changes |
| `platforms/README.md` | Supporting | maintainers, future platform owners | platform folder layout and naming convention | when platform folder conventions change |
| `platforms/macos/README.md` | Supporting | maintainers, future macOS owners | foundation scope and directory purpose for future macOS work | when macOS platform skeleton or scope changes |
| `RELEASE_CHECKLIST.md` | Active | maintainers, release owner | release and package completion checklist | when release process or package completion rules change |
| `CHANGE_CHECKLIST.md` | Active | maintainers, coding agents | non-trivial change completion checklist | when completion rules change |
| `PULL_REQUEST_TEMPLATE.md` | Supporting | maintainers | structured summary template for PR or change review | when review expectations change |
| `DECISION_LOG.md` | Active | maintainers, product owner | index of formal architecture and product decisions | when an ADR is added, superseded, or reclassified |
| `SOURCES_OF_TRUTH.md` | Active | maintainers, coding agents | declares authoritative sources per domain and conflict rules | when ownership of a domain changes |
| `DOCUMENT_REGISTRY.md` | Active | maintainers, coding agents | registry of active documentation set | when a durable doc is added, retired, or materially repurposed |
| `AGENTS.md` | Active | coding agents, maintainers | working rules for future edits and packaging | when workflow rules or documentation discipline changes |
| `DEVLOG.md` | Active | maintainers | chronological record of meaningful changes and package refreshes | after every source or package-facing change |
| `STATUS.md` | Active | maintainers, product owner | current state, stable areas, known risks | when implementation state or risk profile changes |
| `ROADMAP.md` | Active | maintainers, product owner | planned direction and sequencing | when priorities or staged delivery plan changes |
| `ARCHITECTURE.md` | Active | engineers | runtime structure, component responsibilities, boundaries | when runtime design or ownership changes |
| `CONFIGURATION.md` | Active | engineers, support | supported connection formats and generated config behavior | when connection parsing or config generation changes |
| `SESSION_PORTABILITY.md` | Supporting | engineers, product owner | design for future cross-machine workspace portability | when portability strategy changes |
| `WORKSPACE_V2_BACKLOG.md` | Supporting | engineers | staged backlog for workspace-oriented refactor | when extraction backlog or migration plan changes |
| `ADR/README.md` | Supporting | maintainers | conventions for writing and updating ADRs | when ADR process rules change |
| `ADR/ADR-TEMPLATE.md` | Supporting | maintainers | template for new architecture decision records | when ADR structure expectations change |
| `ADR/ADR-0001-managed-browser-workspace-positioning.md` | Supporting | maintainers, product owner | decision record for product positioning | when superseded by a later positioning decision |
| `ADR/ADR-0002-sing-box-tun-runtime.md` | Supporting | engineers | decision record for active network runtime architecture | when superseded by a later runtime decision |
| `ADR/ADR-0003-documentation-governance.md` | Supporting | maintainers | decision record for documentation governance model | when superseded by a later governance decision |
| `ADR/ADR-0004-basic-pro-ui-modes.md` | Supporting | engineers, product owner | decision record for operator/admin mode split | when superseded by a later UX decision |
| `ADR/ADR-0005-release-and-change-enforcement.md` | Supporting | maintainers | decision record for release and change completion enforcement | when superseded by a later governance decision |
| `ADR/ADR-0006-operator-main-screen-and-locked-settings.md` | Supporting | engineers, product owner | decision record for operator-only main screen and separate locked admin settings | when superseded by a later UX decision |
| `ADR/ADR-0007-source-repository-excludes-runtime-artifacts.md` | Supporting | maintainers, release owner | decision record for keeping source history separate from packaged runtime artifacts | when superseded by a later repository-boundary decision |
| `ADR/ADR-0008-single-source-repo-for-windows-and-future-macos.md` | Supporting | maintainers, product owner | decision record for using one source repository for current Windows work and future macOS work | when superseded by a later platform-repository decision |
| `ADR/ADR-0009-platform-specific-source-lives-under-platforms.md` | Supporting | maintainers, platform owners | decision record for platform-specific source living under `platforms/<platform>` while shared logic remains in `libs/` | when superseded by a later platform-layout decision |
| `AgentBrowser_Windows/README.txt` | Package | Windows operator, admin | in-package quick instructions | when package UX or shipped tools change |
| `AgentBrowser_Windows/BASIC_USER_MANUAL.txt` | Package | operator | short user manual for Basic mode | when operator flow changes |
| `AgentBrowser_Windows/PRO_USER_MANUAL.txt` | Package | admin | short user manual for Pro mode | when admin flow or gating changes |

## Exclusions

The following are not part of the governing document set:

- `obj/` file lists
- publish output manifests
- bundled third-party licenses and readmes
- browser vendor documentation shipped inside `AgentBrowser_Windows/`

These may remain in the tree, but they are not documentation sources for project decisions.

## Maintenance Rules

1. Add every durable project document to this registry.
2. If two documents overlap, `SOURCES_OF_TRUTH.md` decides which one is authoritative.
3. If a document becomes obsolete, either remove it or mark it as legacy in this registry and in the document body.
4. Register new ADR files here when they become part of the durable decision set.
5. After changing the registry itself, add a matching entry to `DEVLOG.md`.
