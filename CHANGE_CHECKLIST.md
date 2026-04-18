# Change Checklist

## Purpose

Use this checklist for any non-trivial change before calling the work complete.

## Scope

- [ ] The actual change scope is clear
- [ ] Files touched match the intended scope
- [ ] Unrelated files were not changed without reason

## Decision Check

- [ ] I checked whether this change modifies a durable decision
- [ ] If yes, I updated `DECISION_LOG.md`
- [ ] If yes, I added or updated the relevant ADR

## Documentation Check

- [ ] I updated the domain document that actually owns this behavior
- [ ] I updated `DOCUMENT_REGISTRY.md` if the durable document set changed
- [ ] I updated `SOURCES_OF_TRUTH.md` if source ownership changed
- [ ] I added a `DEVLOG.md` entry

## Implementation Check

- [ ] Code or docs changed as intended
- [ ] Any required build or validation step was run
- [ ] Any unverified area is explicitly called out

## Packaging Check

- [ ] If package artifacts changed, I refreshed `AgentBrowser_Windows/`
- [ ] If package artifacts changed, I refreshed `AgentBrowser_Windows.zip`
- [ ] If package artifacts changed, I reviewed `RELEASE_CHECKLIST.md`

## Completion Gate

The change is not complete until:

- the relevant checklist items are satisfied
- `DEVLOG.md` has the matching entry
- the final response states what changed and what was not verified

