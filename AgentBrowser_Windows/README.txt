AgentBrowser_Windows

1. Run AgentBrowserUI.exe for the main control panel.
2. The main screen is now the operator screen: it shows the current preset, runtime status, connection lamp, and the main action buttons.
3. Operators normally use only:
   - `Test`
   - `Start`
   - `Stop`
4. Open `Settings` only for admin work. Settings are unlocked only if `pro-mode.flag` exists next to AgentBrowserUI.exe or the correct password is entered from environment variable `AGENT_BROWSER_PRO_PASSWORD`.
5. Inside `Settings` you can manage presets, change SOCKS5 or VLESS, test the edited value, and apply it to `core\config.json`.
6. Click Start and accept the Windows UAC prompt, because sing-box TUN needs administrator rights.
7. Click Stop if the browser and tunnel need to be stopped manually.
8. Watch the Connection lamp: green means the live probe succeeded, orange means checking, red means the probe failed.
9. Click Export Bundle to save logs and workspace metadata into a single zip archive for debugging.
10. Click Open Logs to open the package folder with ui.log, start.log, stop.log, and support exports.
11. Run SupportTool.exe if you need a dedicated one-click support bundle export tool outside the UI.
12. For end-user instructions open `BASIC_USER_MANUAL.txt` or `PRO_USER_MANUAL.txt`.

Legacy direct launch:
- Start.exe still works and launches the package with the current core\config.json.
- Stop.exe still stops the browser and sing-box from this package.

Notes:
- The current UI supports SOCKS5 strings and VLESS TCP links with TLS or Reality.
- The UI shows a live Running / Stopped status based on sing-box from this package.
- The operator screen always hides raw connection secrets and shows only a masked summary.
- Admin editing is now in a separate `Settings` window instead of a dropdown `Basic / Pro` switch.
- Settings access can be gated by `pro-mode.flag` or password entry through environment variable `AGENT_BROWSER_PRO_PASSWORD`.
- UI text uses the system language heuristic: Russian systems get Russian labels, otherwise English.
- UI launch and button actions are logged to ui.log next to AgentBrowserUI.exe.
- The connection lamp performs a live outbound probe and shows the current egress IP when the tunnel is healthy.
- Export Bundle and SupportTool.exe collect ui.log, start.log, stop.log, core\config.json, core\sing-box.log, and workspace metadata.
- SupportTool.exe writes its own actions to support-tool.log next to the executable.
- Presets are stored in ui-settings.json and can be duplicated, renamed, deleted, and switched from the admin Settings window.
- `BASIC_USER_MANUAL.txt` is for operators, `PRO_USER_MANUAL.txt` is for admins.
- This build uses a session-wide TUN route while the browser is open.
- Local LAN ranges are excluded from the tunnel in core/config.json.
