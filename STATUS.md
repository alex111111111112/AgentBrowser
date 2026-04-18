# Status

## Current State

The current working branch of the project is the `sing-box` based portable managed browser package.

Repository scope:

- current Windows implementation
- shared source and docs for future macOS work

Implemented:

- portable Windows package in `AgentBrowser_Windows/`
- `Start.exe` with admin manifest and runtime orchestration
- `Stop.exe` with safe path-based process shutdown
- `AgentBrowserUI.exe` with presets, type dropdown, save, test, start, stop
- `ui.log`, `start.log`, `stop.log`, and `core/sing-box.log`
- `SOCKS5` configuration path
- `VLESS TCP` input parsing with `TLS` and `Reality`

Not yet implemented:

- macOS runtime
- macOS packaging
- macOS operator flow

Prepared and buildable:

- `platforms/macos/AgentBrowser.Mac.sln`
- `platforms/macos/app/AgentBrowser.MacShell/`

Current macOS shell scope:

- basic Avalonia desktop shell
- shared workspace and diagnostics wiring
- no tunnel/runtime orchestration yet

Selected first macOS experiment:

- `Avalonia` desktop shell
- no tunnel/runtime parity in the first spike

## Stable Areas

- SOCKS-based startup path
- process ownership checks by executable path
- browser lifecycle tracking in `Start.exe`
- packaged browser launch from `App/Chrome-bin/chrome.exe`
- preset-driven operator flow in `AgentBrowserUI.exe`

## Known Risks

- `VLESS` runtime behavior depends on the remote endpoint and DNS bootstrap correctness on the target Windows machine
- `sing-box check` passing does not mean the tunnel is usable
- `TUN` mode requires admin rights and can fail for local system reasons outside the app
- portable browser bundles are large, which complicates rebuilds and repackaging on a nearly full disk
- browser authentication state may fail to transfer across machines because encrypted browser secrets can be bound to Windows credentials or device state

## Product Direction

Recommended direction:

- managed browser workspace
- isolated operator session
- portable browser + tunnel control
- preset-driven supportable product

Not recommended:

- anti-detect positioning
- promises about stealth or non-detection
- hard commitments around browser fingerprint spoofing

## Legacy Status

Legacy artifacts are kept in the repo but are not the current product:

- Firefox + Xray launcher
- Chromium + Xray launcher
- standalone `stop_xray.exe`

These remain useful only for reference or rollback analysis.

## Recommended Debug Order

1. `ui.log`
2. `start.log`
3. `stop.log`
4. `core/sing-box.log`

## Packaging Status

The packaged output currently exists as:

- `AgentBrowser_Windows/`
- `AgentBrowser_Windows.zip`

These are deployment artifacts, not the canonical place to edit source logic.
