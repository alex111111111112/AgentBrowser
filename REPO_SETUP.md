# Repository Setup

## Purpose

This file defines what belongs in the Git repository and what must stay local, on an external drive, or in release storage.

Use it when:

- preparing the project for GitHub
- deciding whether a file or folder should be committed
- moving the working copy to an external SSD
- preparing release artifacts without polluting git history

## Repository Scope

The Git repository should track:

- source projects under `AgentBrowserUi/`, `SingBoxStart/`, `SingBoxStop/`, `SupportTool/`
- shared libraries under `libs/`
- durable documentation and governance files in the repository root and `ADR/`
- lightweight package manuals in `AgentBrowser_Windows/`
- the default solution file `AgentBrowser.sln`
- future macOS source projects when that implementation starts

The Git repository should not track:

- packaged runtime binaries inside `AgentBrowser_Windows/`
- `AgentBrowser_Windows.zip`
- downloaded vendor resources in `resurs/`
- local build output under `bin/` and `obj/`
- local logs and support bundles

`.gitignore` is the enforcement layer for this split.

## Default Working Model

Use the repository as the source-of-truth for:

- code
- docs
- build orchestration
- release instructions

Use local disk, an external SSD, or GitHub Releases for:

- packaged Windows bundles
- zipped release candidates
- downloaded browser or tunnel vendor payloads

Do not treat the packaged runtime directory as the source repository.

## Solution Entry Point

Open `AgentBrowser.sln` as the default entry point for active development.

The solution intentionally contains the active managed browser workspace projects and shared libraries.

Legacy Xray prototypes remain in the tree for reference, but they are not part of the default solution.

Future macOS projects may live in the same repository, but they should be added only when there is a real implementation track.

## GitHub Preparation Flow

Recommended first-time sequence:

1. run `git init -b main`
2. verify that `git status` does not include packaged runtime binaries or downloaded vendor bundles
3. add the GitHub remote
4. commit source and docs only
5. push to GitHub
6. publish release bundles separately through GitHub Releases or external storage

## External Drive Workflow

Using an external SSD for the working copy is acceptable.

Preferred split:

- Git working copy on the external SSD
- release zips and runtime bundles on the same SSD outside normal git history

Practical notes:

- use `exFAT` if the drive must move between macOS and Windows
- use `APFS` if the drive is macOS-only and you want better local behavior
- avoid running the working copy from a slow HDD if repeated `dotnet publish` is part of the flow

## Release Artifact Policy

If a customer-ready or operator-ready package is needed:

- rebuild `AgentBrowser_Windows/`
- rebuild `AgentBrowser_Windows.zip`
- keep the archive out of normal git history
- attach it to a release or store it in external artifact storage

## Change Rule

If the repository boundary changes again, update at minimum:

- `README.md`
- `AGENTS.md`
- `SOURCES_OF_TRUTH.md`
- `DOCUMENT_REGISTRY.md`
- `DECISION_LOG.md`
- the relevant ADR
- `DEVLOG.md`
