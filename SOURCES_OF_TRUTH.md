# Sources Of Truth

## Purpose

This file defines the authoritative source for each project domain.

Use it when:

- two documents disagree
- code and docs appear out of sync
- a maintainer needs to know which file must be updated first

## Precedence Rules

1. Runtime behavior: source code wins over documentation.
2. Domain ownership: this file decides which document or code path is authoritative.
3. Document inventory: `DOCUMENT_REGISTRY.md` decides whether a document is official and active.
4. Change history: `DEVLOG.md` is the authoritative timeline of project edits.
5. Packaged operator instructions: files under `AgentBrowser_Windows/` are authoritative only for shipped end-user operation, not for source design.
6. Formal architecture decisions: ADRs are authoritative for accepted decision rationale until explicitly superseded.

## Domain Map

| Domain | Primary source of truth | Secondary references | Notes |
| --- | --- | --- | --- |
| Product positioning and commercial framing | `README.md` | `STATUS.md`, `AGENTS.md` | Project is a managed browser workspace, not an anti-detect product |
| Current implementation target | `README.md` section "Current Source Of Truth" | `AGENTS.md`, `STATUS.md` | Windows is active; future macOS work shares the repo but is not yet a delivered runtime |
| Runtime orchestration behavior | `SingBoxStart/Program.cs`, `SingBoxStop/Program.cs`, `libs/AgentBrowser.Sessions/*` | `ARCHITECTURE.md` | Code is authoritative if docs drift; default mode is browser-only proxy, `System TUN` is advanced |
| UI behavior and operator controls | `AgentBrowserUi/Program.cs` | `AgentBrowser_Windows/README.txt`, package manuals | UI code is authoritative, including runtime-mode selection and conditional elevation |
| Connection parsing and config generation | `libs/AgentBrowser.Config/*` | `CONFIGURATION.md`, `AgentBrowserUi/Program.cs` | Config library is authoritative for both `Browser-only Proxy` and `System TUN` generation |
| Workspace layout and session state files | `libs/AgentBrowser.Workspaces/*` | `ARCHITECTURE.md`, `WORKSPACE_V2_BACKLOG.md` | Includes `workspaces/default/*` conventions |
| Support bundle contents and diagnostics export | `libs/AgentBrowser.Support/*`, `SupportTool/Program.cs` | `AgentBrowser_Windows/README.txt` | Support runtime behavior is code-driven |
| Repository hygiene and GitHub publishing | `REPO_SETUP.md`, `.gitignore` | `README.md`, `AGENTS.md` | Source repo tracks code and docs; runtime bundles stay out of normal git history |
| Platform folder layout | `platforms/README.md`, `platforms/macos/README.md` | `README.md`, `ARCHITECTURE.md` | Shared logic stays in `libs/`; platform-local work goes under `platforms/<platform>/` |
| First macOS experiment choice | `MACOS_RUNTIME_OPTIONS.md`, `ADR/ADR-0010-first-macos-experiment-uses-avalonia-shell.md` | `ROADMAP.md`, `STATUS.md` | Current first spike is an `Avalonia` shell, not runtime parity |
| Current macOS shell behavior | `platforms/macos/app/AgentBrowser.MacShell/*` | `platforms/macos/README.md`, `ARCHITECTURE.md` | This is the implementation source for the first buildable macOS shell |
| Change completion gate | `CHANGE_CHECKLIST.md`, `AGENTS.md` | `DEVLOG.md`, `PULL_REQUEST_TEMPLATE.md` | Checklist and contributor rules define when a change is done |
| Release readiness and package sign-off | `RELEASE_CHECKLIST.md` | `AgentBrowser_Windows/README.txt`, `DEVLOG.md` | Release checklist governs package completion |
| Current project health and risks | `STATUS.md` | `README.md`, `DEVLOG.md` | `STATUS.md` is the canonical summary |
| Durable architecture and product decisions | `DECISION_LOG.md`, `ADR/*.md` | `README.md`, `ROADMAP.md` | `DECISION_LOG.md` is the index, ADRs hold the full rationale |
| Future direction and sequencing | `ROADMAP.md` | `WORKSPACE_V2_BACKLOG.md`, `SESSION_PORTABILITY.md` | Roadmap owns near/mid-term direction |
| Cross-machine session portability design | `SESSION_PORTABILITY.md` | `README.md` profile portability section | This is forward-looking, not current runtime behavior |
| Package operation for Basic users | `AgentBrowser_Windows/BASIC_USER_MANUAL.txt` | `AgentBrowser_Windows/README.txt` | Basic manual wins for operator instructions |
| Package operation for Pro users | `AgentBrowser_Windows/PRO_USER_MANUAL.txt` | `AgentBrowser_Windows/README.txt` | Pro manual wins for admin instructions |
| Documentation rules for future edits | `AGENTS.md` | `DOCUMENT_REGISTRY.md`, `DEVLOG.md` | `AGENTS.md` governs contributor workflow |
| Chronological change record | `DEVLOG.md` | none | Every source or package-facing change should be logged here |

## Conflict Resolution

If two sources disagree:

1. check whether one of them is outside the active registry
2. follow this file for domain ownership
3. prefer code over docs for runtime behavior
4. update the stale document immediately
5. record the correction in `DEVLOG.md`

## Required Round-Trip

For mature documentation hygiene, the loop is:

1. implement or change code
2. update the relevant domain document
3. update `DECISION_LOG.md` if a durable decision changed
4. update `DOCUMENT_REGISTRY.md` if the document set changed
5. update `DEVLOG.md`
