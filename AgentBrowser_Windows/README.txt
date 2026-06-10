AgentBrowser_Windows

1. Run AgentBrowserUI.exe for the main control panel.
2. The main screen is now the operator screen: it shows the current preset, runtime status, connection lamp, and the main action buttons.
3. Operators normally use only:
   - `Test`
   - `Start`
   - `Stop`
4. Open `Settings` only for admin work. By default `Settings` opens directly. A password prompt appears only if an admin has configured environment variable `AGENT_BROWSER_PRO_PASSWORD`. File `pro-mode.flag` next to AgentBrowserUI.exe still bypasses the lock explicitly.
5. Inside `Settings` you can manage presets, change SOCKS5 or VLESS, choose the runtime mode, test the edited value, and apply it to `core\config.json`.
6. Default runtime mode is `Browser-only Proxy (Recommended)`.
   - only the bundled browser goes through the configured proxy or VLESS route
   - other browsers and applications on the same PC should keep using the normal network path
7. Advanced runtime mode is `System TUN`.
   - this mode can route the whole machine through sing-box while the session is active
   - it may require a Windows UAC confirmation when Start.exe or Stop.exe is launched
   - use it only when you explicitly need system-wide routing
8. In `Browser-only Proxy` mode, click Start and wait for the bundled browser to open.
9. In `System TUN` mode, click Start and accept the Windows UAC prompt if it appears.
   - if the user clicks No or closes the UAC dialog, Start.exe will not launch
   - on a non-admin account, Windows may ask for local administrator credentials
   - if the Start button does not trigger the expected elevation flow, you can right-click Start.exe in the package folder, choose `Run as administrator`, and keep AgentBrowserUI.exe open to watch the status change
10. Click Stop if the browser and tunnel need to be stopped manually.
11. Watch the Connection lamp: green means the live probe succeeded, orange means checking, red means the probe failed.
12. Click Export Bundle to save logs and workspace metadata into a single zip archive for debugging.
13. Click Open Logs to open the package folder with ui.log, start.log, stop.log, and support exports.
14. Run SupportTool.exe if you need a dedicated one-click support bundle export tool outside the UI.
15. For end-user instructions open `BASIC_USER_MANUAL.txt` or `PRO_USER_MANUAL.txt`.

Legacy direct launch:
- Start.exe still works and launches the package with the current core\config.json.
- Stop.exe still stops the browser and sing-box from this package.
- AgentBrowserUI.exe can stay open while Start.exe is launched manually; the UI will reflect the running session after Start.exe succeeds.

Notes:
- The current UI supports SOCKS5 strings and VLESS TCP links with TLS or Reality.
- The UI shows a live Running / Stopped status based on sing-box from this package.
- The operator screen always hides raw connection secrets and shows only a masked summary.
- Admin editing is now in a separate `Settings` window instead of a dropdown `Basic / Pro` switch.
- Settings opens directly unless an admin has explicitly configured `AGENT_BROWSER_PRO_PASSWORD`.
- If `AGENT_BROWSER_PRO_PASSWORD` is configured, clicking `Settings` opens the password prompt.
- `pro-mode.flag` next to AgentBrowserUI.exe still bypasses the lock for local admin access.
- UI text uses the system language heuristic: Russian systems get Russian labels, otherwise English.
- UI launch and button actions are logged to ui.log next to AgentBrowserUI.exe.
- In `Browser-only Proxy` mode the connection lamp probes through the local SOCKS listener used by the bundled browser.
- In `System TUN` mode the connection lamp probes through the system-routed session.
- Export Bundle and SupportTool.exe collect ui.log, start.log, stop.log, core\config.json, core\sing-box.log, and workspace metadata.
- SupportTool.exe writes its own actions to support-tool.log next to the executable.
- Presets are stored in ui-settings.json and can be duplicated, renamed, deleted, switched, and assigned a runtime mode from the admin Settings window.
- `BASIC_USER_MANUAL.txt` is for operators, `PRO_USER_MANUAL.txt` is for admins.
- `Browser-only Proxy` is the default and recommended mode for operator use.
- `System TUN` remains available as an advanced mode when machine-wide routing is required.
