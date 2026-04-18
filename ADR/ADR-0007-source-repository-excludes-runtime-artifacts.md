# ADR-0007: Source Repository Excludes Runtime Artifacts

## Status

Accepted

## Date

2026-04-18

## Context

The working directory contains both source code and large local artifacts:

- packaged Windows runtime contents in `AgentBrowser_Windows/`
- release zip files
- downloaded vendor resources in `resurs/`
- local `bin/` and `obj/` output

That layout is practical for one machine, but it is a poor default for GitHub:

- repository size grows too fast
- clone and fetch become unnecessarily heavy
- release candidates and vendor payloads pollute source history
- contributors cannot tell which files are authoritative source versus local runtime state

The project needs a durable rule for separating source history from local or release-only artifacts.

## Decision

The repository will track source code, shared libraries, durable docs, governance files, and a default solution file.

The repository will not track normal packaged runtime artifacts or downloaded vendor bundles, including:

- runtime binaries inside `AgentBrowser_Windows/`
- `AgentBrowser_Windows.zip`
- `resurs/`
- local `bin/` and `obj/`
- local logs and support bundles

This boundary is enforced by `.gitignore` and documented in `REPO_SETUP.md`.

The default developer entry point is `AgentBrowser.sln`, which contains the active projects and shared libraries only.

Release bundles remain valid project outputs, but they live outside normal git history and should be distributed through release storage instead.

## Consequences

Positive:

- the GitHub repository stays small and reviewable
- the source boundary becomes explicit
- the working copy can still keep local runtime bundles for packaging and testing
- external drives can hold both the git checkout and large release artifacts without forcing those artifacts into git

Tradeoffs:

- a fresh clone is not a runnable Windows package by itself
- release assembly becomes a documented step instead of an implicit property of the repo
- maintainers must keep package manuals and release process docs aligned with the ignored runtime artifacts

## Follow-Up

- keep `.gitignore` aligned with actual runtime output patterns
- use GitHub Releases or external storage for packaged bundles
- update governance docs if the repository boundary changes again
