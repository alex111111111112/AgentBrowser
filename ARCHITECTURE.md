# Architecture

## Purpose

The project packages a controlled Windows browser environment that launches a bundled Chromium runtime behind a local `sing-box` tunnel, with a simple UI for changing connection settings.

It is best understood as a managed browser runtime, not as an anti-detect browser core.

## Active Runtime Path

```text
AgentBrowserUI.exe
  -> writes core/config.json
  -> launches Start.exe
      -> validates files
      -> runs sing-box check
      -> starts core/sing-box.exe in TUN mode
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

- no direct process ownership of browser or tunnel
- delegates runtime orchestration to helper executables

### `SingBoxStart`

Responsibilities:

- require administrator privileges
- verify required files exist
- verify `sing-box` configuration before launch
- start `sing-box`
- start bundled browser with local profile
- wait for browser session end
- stop `sing-box` if it was started by this process
- write `start.log`

Important behavior:

- skips duplicate launches using a mutex
- does not kill pre-existing `sing-box` started outside the current run
- only tracks browser binaries from the same package folder

### `SingBoxStop`

Responsibilities:

- require administrator privileges
- stop bundled Chrome from the same package only
- stop `sing-box` from the same package only
- tolerate transient process inspection failures
- write `stop.log`

### `core/config.json`

Responsibilities:

- define DNS behavior
- define `TUN` inbound
- define active outbound
- define traffic routing strategy

The UI rewrites this file when the user saves a preset.

## File Ownership

Source projects:

- `AgentBrowserUi/*`
- `SingBoxStart/*`
- `SingBoxStop/*`

Packaged runtime:

- `AgentBrowser_Windows/*`

Legacy artifacts:

- root `Program.cs`
- `ChromiumLauncher/*`
- `StopXray/*`

## Security And Scope Boundaries

- The package is Windows-only at runtime.
- `Start.exe` and `Stop.exe` require elevated rights because `TUN` mode changes routing.
- Process matching is path-based to avoid killing unrelated system browser or tunnel processes.
- The browser profile is local to the package folder.
- Browser authentication state is not guaranteed to be portable across Windows machines because sensitive browser secrets may be protected by Windows-bound encryption.

## Operational Caveats

- `sing-box check` validates syntax, not real network reachability.
- Connectivity debugging should use `core/sing-box.log`, not only `start.log`.
- Domain-based `VLESS` needs correct DNS bootstrap behavior; config generation must avoid proxying resolution of the proxy server through itself.
- The browser profile directory is not sufficient by itself to guarantee cross-machine session migration.
