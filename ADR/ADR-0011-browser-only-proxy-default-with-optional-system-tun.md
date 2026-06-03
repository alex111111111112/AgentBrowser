# ADR-0011: Browser-Only Proxy Default With Optional System TUN

- Status: Accepted
- Date: 2026-04-18

## Context

The earlier Windows runtime treated `sing-box` `TUN` mode as the default network path.

That made the package work as a system-wide route while the session was active:

- the bundled browser used the tunnel
- other browsers also used the tunnel
- unrelated applications on the same machine could become slower or route through the remote server

This is the wrong default for the product.

The product is a managed browser workspace, not a machine-wide VPN client.

## Decision

Adopt a dual-mode Windows runtime model:

- default mode: `Browser-only Proxy`
- advanced mode: `System TUN`

Behavior:

- `Browser-only Proxy` starts `sing-box` with a local SOCKS inbound on `127.0.0.1:1080`
- the bundled browser is launched with an explicit `--proxy-server=socks5://127.0.0.1:1080`
- other applications on the machine are not intentionally rerouted in the default mode
- `System TUN` remains available as an explicit advanced runtime mode
- `Start.exe` and `Stop.exe` run as `asInvoker`
- the UI requests elevation with `runas` only when the selected runtime mode is `System TUN`

## Consequences

Positive:

- safer default operator experience
- no system-wide routing side effect in the common case
- no admin prompt required for the default browser-only mode
- clearer product boundary between browser workspace and full-machine routing

Negative:

- two runtime modes must now be documented and tested
- browser-only connectivity health must be probed through the local proxy path, not the system path
- `System TUN` becomes an advanced path that is more sensitive to docs drift and support mistakes
