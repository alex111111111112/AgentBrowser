# Configuration

## Supported Input Formats

The UI currently supports two connection types.

## SOCKS5

Accepted formats:

```text
host:port
host:port:user:pass
```

Example:

```text
50.118.198.32:64011:Ek5R3AJg:prGqb63R
```

Result:

- `type: socks`
- optional `username` and `password`
- outbound tag `proxy`

## VLESS

Accepted format:

```text
vless://uuid@host:port?encryption=none&flow=xtls-rprx-vision&type=tcp&security=reality&sni=host&fp=chrome&pbk=...&sid=...
```

Current parser assumptions:

- only `type=tcp` is supported
- `security=tls` or `security=reality` is supported
- `reality` requires both `pbk` and `sid`
- `fp` maps to `utls.fingerprint`
- `sni` maps to `tls.server_name`

Example:

```text
vless://ca927be1-c2cb-4f52-a941-4480049adfe1@usa6.un1.pro:443?encryption=none&flow=xtls-rprx-vision&type=tcp&security=reality&sni=usa6.un1.pro&fp=chrome&pbk=Ef3MlZmKu6OYxX7EotP_-FJPdfRmhrOZQpqqPAVAzz4&sid=6ba85179e30d4fc2
```

## Runtime Modes

Each preset now has a runtime mode in addition to the connection type.

### Browser-only Proxy

Default and recommended mode.

Generated behavior:

- local SOCKS inbound on `127.0.0.1:1080`
- bundled browser launched with `--proxy-server=socks5://127.0.0.1:1080`
- other applications on the same machine are not intentionally rerouted

### System TUN

Advanced mode.

Generated behavior:

- `tun` inbound
- `auto_route = true`
- `strict_route = true`
- broader machine-wide routing while the session is active

## Generated `sing-box` Base Config

The UI builds a common base and swaps the active outbound and inbound/runtime shape:

- `log.output = sing-box.log`
- `dns` section
- inbound and route rules depend on runtime mode
- `final = proxy`

## Default Files

Editable at runtime:

- `AgentBrowser_Windows/ui-settings.json`
- `AgentBrowser_Windows/core/config.json`

Generated and diagnostic:

- `AgentBrowser_Windows/ui.log`
- `AgentBrowser_Windows/start.log`
- `AgentBrowser_Windows/stop.log`
- `AgentBrowser_Windows/core/sing-box.log`

## Important Runtime Notes

- `Test` in the UI only runs `sing-box check`
- `Test` does not confirm that remote credentials are valid
- `Browser-only Proxy` is the default operator path
- `System TUN` can affect other browsers and applications on the machine while the session is active
- if a config starts but traffic fails, inspect `core/sing-box.log`
- if the UI button appears to do nothing, inspect `ui.log`
