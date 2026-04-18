# ADR-0001: Managed Browser Workspace Positioning

- Status: Accepted
- Date: 2026-04-18

## Context

The project started from launcher-style browser bundles and could drift toward anti-detect positioning.

That direction creates:

- larger engineering scope
- higher maintenance burden
- higher legal, payment, and trust risk

The implemented system is much closer to a managed browser runtime with profile isolation, tunnel orchestration, presets, and diagnostics.

## Decision

Position the product as a managed browser workspace, not an anti-detect browser.

Preferred language:

- managed browser workspace
- isolated session browser
- portable browser + tunnel controller
- operator-friendly browser runtime

Avoid default positioning such as:

- anti-detect browser
- stealth browser
- undetectable browser

## Consequences

Positive:

- aligns messaging with actual implementation
- keeps roadmap realistic
- reduces pressure to implement fingerprint spoofing claims

Negative:

- narrows some commercial narratives
- excludes “stealth” positioning as a default sales path

