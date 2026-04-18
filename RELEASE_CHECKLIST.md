# Release Checklist

## Purpose

Use this checklist before calling a package refresh or release complete.

This applies to:

- `AgentBrowser_Windows/`
- `AgentBrowser_Windows.zip`
- any release candidate passed to an operator or customer

## Release Readiness

- [ ] Release scope is clear
- [ ] No unresolved blocker is intentionally hidden
- [ ] Current package behavior matches the stated release scope

## Documentation

- [ ] `README.md` reflects the current product state if product-facing behavior changed
- [ ] relevant domain docs are updated
- [ ] `DOCUMENT_REGISTRY.md` is updated if the document set changed
- [ ] `SOURCES_OF_TRUTH.md` is updated if domain ownership changed
- [ ] `DECISION_LOG.md` and a new ADR were added if the change modified a durable decision
- [ ] `DEVLOG.md` contains an entry for the release-related change
- [ ] package-facing docs in `AgentBrowser_Windows/` are updated if operator behavior changed

## Build And Publish

- [ ] required projects build successfully
- [ ] required projects publish successfully
- [ ] refreshed binaries were copied into `AgentBrowser_Windows/`
- [ ] stale binaries or mismatched outputs were not left behind

## Package Integrity

- [ ] package contains the expected executables
- [ ] package contains the expected runtime files
- [ ] package contains the expected manuals and README
- [ ] archive was rebuilt after package changes
- [ ] archive does not contain `__MACOSX` or `._*` junk

## Functional Smoke Checks

- [ ] UI starts
- [ ] Start path works or is explicitly unverified and called out
- [ ] Stop path works or is explicitly unverified and called out
- [ ] support bundle export path is available
- [ ] logs are produced in expected locations

## Release Notes

- [ ] major changes are summarized
- [ ] known risks are stated plainly
- [ ] unverified areas are stated plainly

## Completion Rule

Do not call the release complete until:

- this checklist is satisfied
- the relevant `DEVLOG.md` entry exists
- the final archive or package path is confirmed

