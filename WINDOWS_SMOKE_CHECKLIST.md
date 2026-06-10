# Windows Smoke Checklist

## Purpose

Use this checklist when a refreshed `AgentBrowser_Windows.zip` needs manual verification on a real Windows machine.

This is a smoke test, not a full QA pass. It answers one question: can the refreshed package start, stop, log, and export diagnostics on Windows without obvious packaging breakage?

## Artifact Under Test

Expected artifact:

- `/Volumes/Yablocko/AgentBrowser/releases/AgentBrowser_Windows.zip`

Current refreshed artifact SHA256:

- `24bf57f1614c0cbc8be91f515657e81f596989f2e46c8aa4b800a48c159b737d`

If the zip is transferred to Windows, optionally verify it in PowerShell:

```powershell
Get-FileHash .\AgentBrowser_Windows.zip -Algorithm SHA256
```

## Tester Rules

- Do not send raw connection strings, tokens, passwords, cookies, or full `core\config.json` contents back in chat.
- Mask connection secrets in screenshots.
- Prefer screenshots of UI state, file names, status lamps, and error dialogs.
- Keep the extracted folder path simple, for example `C:\Temp\AgentBrowser_Windows`.
- Do not run the `System TUN` path unless admin rights are available and the owner explicitly wants that mode tested.

## Required Smoke

### 1. Extract

1. Extract `AgentBrowser_Windows.zip`.
2. Confirm the extracted folder contains:
   - `AgentBrowserUI.exe`
   - `Start.exe`
   - `Stop.exe`
   - `SupportTool.exe`
   - `README.txt`
   - `BASIC_USER_MANUAL.txt`
   - `PRO_USER_MANUAL.txt`
   - `core\sing-box.exe`
   - `core\config.json`
   - `ui-settings.json`

Expected: all files exist.

### 2. Launch UI

1. Run `AgentBrowserUI.exe`.
2. If Windows SmartScreen or antivirus blocks it, record the exact message and stop.
3. Confirm the main window opens.
4. Confirm the main screen shows:
   - current preset or masked connection summary;
   - runtime status;
   - connection lamp;
   - `Test`, `Start`, `Stop`, `Settings`, `Export Bundle`, `Open Logs`.

Expected: UI opens without crash.

### 3. Settings Readiness

1. Click `Settings`.
2. Confirm it opens directly unless `AGENT_BROWSER_PRO_PASSWORD` is configured on that Windows machine.
3. Confirm connection type choices match current source behavior:
   - `SOCKS5`
   - `VLESS`
4. Confirm there is no `HTTP` option.
5. Close Settings without changing secrets unless the owner explicitly asks for a config edit.

Expected: Settings opens and shows no stale HTTP option.

### 4. Browser-Only Proxy Start

1. Ensure runtime mode is `Browser-only Proxy (Recommended)`.
2. Click `Start`.
3. Wait for bundled browser launch.
4. Confirm UI state changes to running or connected.
5. If connection lamp turns green, record "probe green".
6. If connection lamp is red, record the UI message and continue to logs.

Expected: `Start` does not crash the UI and attempts to launch bundled browser.

### 5. Stop

1. Click `Stop`.
2. Confirm UI returns to stopped state.
3. Confirm bundled browser closes or is clearly being stopped.

Expected: `Stop` does not crash the UI.

### 6. Logs

1. Click `Open Logs`.
2. Confirm these files exist when actions were attempted:
   - `ui.log`
   - `start.log`
   - `stop.log`
   - `core\sing-box.log` if `sing-box` started

Expected: logs are produced in expected locations.

### 7. Support Bundle

1. Click `Export Bundle`.
2. Save the bundle to a temporary folder.
3. Confirm a zip file is created.
4. Do not upload it publicly. Share only through the approved private channel.

Expected: support bundle export works.

### 8. Dedicated SupportTool

1. Run `SupportTool.exe`.
2. Save a support bundle.
3. Confirm a zip file is created.

Expected: standalone support tool can export diagnostics.

## Optional Advanced Smoke

Run this only if the owner wants `System TUN` tested on a Windows admin account.

1. Open `Settings`.
2. Switch runtime mode to `System TUN`.
3. Save/apply config.
4. Click `Start`.
5. Confirm Windows UAC appears when required.
6. Accept UAC.
7. Confirm UI does not crash and status/logs reflect the attempt.
8. Click `Stop`.
9. Restore the intended runtime mode after testing.

Expected: admin flow is understandable and either starts correctly or fails with clear logs.

## Pass Criteria

Required smoke is pass if:

- zip extracts;
- UI launches;
- Settings opens;
- connection types are not stale;
- Browser-only Start attempts to launch bundled browser;
- Stop works without crash;
- logs are created;
- support bundle export works;
- SupportTool export works.

Windows smoke remains blocked or failed if:

- no Windows machine was used;
- UI cannot launch;
- helper executables are missing or blocked;
- logs are not created;
- support bundle cannot be exported;
- the package still shows stale HTTP connection type.

## Report Back Template

```text
Windows smoke result: pass / fail / blocked
Windows version:
User account type: admin / non-admin
Artifact SHA256:
Extract path:

UI launch:
Settings:
Connection types shown:
Browser-only Start:
Stop:
Logs present:
Export Bundle:
SupportTool:
System TUN optional test: not run / pass / fail

Issues:
- ...

Screenshots/log bundle location:
- ...
```
