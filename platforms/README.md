# Platforms

This directory holds platform-specific implementation tracks.

Rules:

- keep reusable logic in `libs/`
- keep product and governance docs in the repository root and `ADR/`
- use `platforms/<platform>/` only for platform-local application, runtime, and packaging work

Current state:

- `platforms/macos/` exists as an empty foundation skeleton
- there is no dedicated `platforms/windows/` source tree yet because the active Windows implementation still lives in the existing root projects

Expected platform layout:

```text
platforms/
├── macos/
│   ├── app/
│   ├── runtime/
│   └── packaging/
└── windows/
    ├── app/
    ├── runtime/
    └── packaging/
```

`windows/` should be created only if a dedicated platform source split becomes necessary.
