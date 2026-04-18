# macOS Foundation

This directory is the placeholder for future macOS implementation work.

It is intentionally empty of runtime code right now.

Purpose:

- reserve stable naming for the macOS track
- define where app, runtime, and packaging work should live
- avoid scattering future macOS experiments across the repository root

Current skeleton:

```text
platforms/macos/
├── app/
├── runtime/
└── packaging/
```

Scope of this foundation step:

- no runtime implementation
- no launcher
- no browser packaging
- no tunnel integration

When real macOS work starts:

- put app-facing code in `app/`
- put platform-local orchestration or helper code in `runtime/`
- put packaging, bundle, and release assembly work in `packaging/`
- keep shared parsing, diagnostics, session, workspace, and support logic in `libs/`
