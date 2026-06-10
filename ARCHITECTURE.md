# Architecture

## Purpose

The project packages a controlled Windows browser environment that launches a bundled Chromium runtime through `sing-box`, with a simple UI for changing connection settings and runtime mode.

It is best understood as a managed browser runtime, not as an anti-detect browser core.

The current runtime is Windows-only, but the repository is allowed to host future platform tracks as long as shared logic remains in `libs/` and platform-local work stays under `platforms/<platform>/`.

## Active Runtime Path

```text
AgentBrowserUI.exe
  -> writes core/config.json
  -> launches Start.exe
      -> validates files
      -> runs sing-box check
      -> starts core/sing-box.exe in the selected runtime mode
      -> launches bundled browser
      -> waits for browser exit
      -> stops sing-box if Start.exe started it
```

Manual shutdown path:

```text
Stop.exe
  -> finds bundled chrome.exe processes from this folder
  -> kills them
  -> finds core/sing-box.exe from this folder
  -> kills it
```

## Components

### `AgentBrowserUi`

Responsibilities:

- show minimal control panel
- store presets in `ui-settings.json`
- parse `SOCKS5` and `VLESS` input formats
- generate `core/config.json`
- run `sing-box check`
- launch `Start.exe` and `Stop.exe`
- log UI actions to `ui.log`

Constraints:

- no direct process ownership of browser or `sing-box`
- delegates runtime orchestration to helper executables

### `SingBoxStart`

Responsibilities:

- verify required files exist
- verify `sing-box` configuration before launch
- start `sing-box`
- start bundled browser with local profile and explicit proxy args when `Browser-only Proxy` mode is selected
- wait for browser session end
- stop `sing-box` if it was started by this process
- write `start.log`

Important behavior:

- skips duplicate launches using a mutex
- does not kill pre-existing `sing-box` started outside the current run
- only tracks browser binaries from the same package folder
- validates `wintun.dll` only when the current config uses `System TUN`

### `SingBoxStop`

Responsibilities:

- stop bundled Chrome from the same package only
- stop `sing-box` from the same package only
- tolerate transient process inspection failures
- write `stop.log`

### `platforms/macos/app/AgentBrowser.MacShell`

Responsibilities:

- prove a launchable macOS desktop shell
- reuse shared workspace and diagnostics libraries
- choose a writable macOS-local app-support path
- define the seam between future macOS app, runtime, and packaging layers

Constraints:

- no tunnel orchestration
- no entitlement or privilege handling
- no packaging or notarization work in this first shell

### `core/config.json`

Responsibilities:

- define DNS behavior
- define either `Browser-only Proxy` inbound or `System TUN` inbound
- define active outbound
- define traffic routing strategy

The UI rewrites this file when the user saves a preset.

## File Ownership

Source projects:

- `AgentBrowserUi/*`
- `SingBoxStart/*`
- `SingBoxStop/*`
- `libs/*`

Platform foundation:

- `platforms/macos/*`
- `platforms/macos/app/AgentBrowser.MacShell/*`

Packaged runtime:

- `AgentBrowser_Windows/*`

Legacy artifacts:

- root `Program.cs`
- `ChromiumLauncher/*`
- `StopXray/*`

## Security And Scope Boundaries

- The package is Windows-only at runtime.
- `Browser-only Proxy` is the default operator path and should not affect other applications on the machine.
- `System TUN` remains available as an advanced mode and may require elevation because it changes machine-wide routing.
- Process matching is path-based to avoid killing unrelated system browser or tunnel processes.
- The browser profile is local to the package folder.
- Browser authentication state is not guaranteed to be portable across Windows machines because sensitive browser secrets may be protected by Windows-bound encryption.

## Operational Caveats

- `sing-box check` validates syntax, not real network reachability.
- Connectivity debugging should use `core/sing-box.log`, not only `start.log`.
- Domain-based `VLESS` needs correct DNS bootstrap behavior; config generation must avoid proxying resolution of the proxy server through itself.
- The browser profile directory is not sufficient by itself to guarantee cross-machine session migration.
