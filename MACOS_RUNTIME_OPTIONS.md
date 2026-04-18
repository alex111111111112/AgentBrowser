# macOS Runtime Options

## Purpose

This document narrows the first real macOS experiment to a concrete stack and scope.

The goal is not to promise a full macOS product yet. The goal is to choose the least-wrong first implementation path for this repository.

## Current Constraints

The project already has:

- shared `.NET` libraries under `libs/`
- a Windows-first operator model
- helper-driven runtime orchestration
- a need for future macOS work in the same repository

The project does not yet have:

- a macOS UI runtime
- a macOS packaging path
- a macOS tunnel strategy
- a macOS privilege or entitlement model

That means the first macOS experiment should optimize for:

- maximum reuse of existing `.NET` logic
- minimum platform risk
- a real desktop shell, not a theoretical platform abstraction

## Candidate A: Avalonia Desktop On macOS

Summary:

- cross-platform desktop UI in `.NET`
- native macOS support through Avalonia's macOS backend
- best fit for reusing current shared `.NET` libraries

Pros:

- strongest reuse of the current codebase
- keeps one language and one build toolchain for shared logic
- closer to a desktop operator app than a mobile-adapted stack
- low-friction place to test presets, logs, settings screens, and support flows

Cons:

- not the same UI framework as the current Windows WPF-style app
- macOS packaging, signing, and notarization still remain separate work
- tunnel and privilege work will still need platform-specific design

## Candidate B: .NET MAUI With Mac Catalyst

Summary:

- official Microsoft path for running a `.NET MAUI` app on macOS via Mac Catalyst

Pros:

- Microsoft-supported stack
- keeps the app in `.NET`
- viable if the product later needs a stronger multi-device UI model

Cons:

- Mac Catalyst is still a translated Apple stack rather than a dedicated desktop-first path
- less aligned with the current operator-desktop feel
- more moving pieces for a first experiment than needed
- does not solve the real macOS tunnel and entitlement problem

## Candidate C: Native SwiftUI Or AppKit Plus Helper

Summary:

- native macOS UI with a separate helper or service layer

Pros:

- most native long-term macOS fit
- best alignment with eventual macOS-specific runtime or entitlement work

Cons:

- weakest reuse of current application code
- highest implementation cost for the first experiment
- duplicates product logic earlier than necessary

## Decision

The first macOS experiment should use:

- `Avalonia` for the desktop shell
- existing shared `.NET` libraries where possible
- a structure-first implementation inside `platforms/macos/`

The first experiment should explicitly not attempt:

- `TUN` integration
- full `sing-box` orchestration
- privilege escalation
- final packaging/signing/notarization

## First Experiment Scope

The first macOS spike should prove only these things:

1. a launchable desktop shell under `platforms/macos/app/`
2. a basic operator-style window layout
3. wiring to shared non-platform-specific libraries where useful
4. macOS-local logging path decisions
5. a clear seam where future runtime and packaging layers will plug in

Success for the first experiment is:

- a running macOS shell
- a stable project structure
- a clear next-step decision for runtime integration

Success is not:

- a full macOS release
- browser/tunnel parity with Windows
- App Store readiness

## Why This Is The Least-Wrong First Step

It proves the highest-value unknown first:

- can this repo host a real macOS desktop shell without splitting the product or abandoning shared `.NET` code?

If the answer is yes, later runtime work can be added behind that shell.

If the answer is no, the project can switch to a native macOS UI before spending effort on tunnel integration and packaging.

## References

- Avalonia macOS platform guide: [docs.avaloniaui.net](https://docs.avaloniaui.net/docs/platform-specific-guides/macos)
- .NET MAUI Mac Catalyst overview: [learn.microsoft.com](https://learn.microsoft.com/dotnet/maui/mac-catalyst/)
- Apple Network Extension overview: [developer.apple.com](https://developer.apple.com/documentation/networkextension)
