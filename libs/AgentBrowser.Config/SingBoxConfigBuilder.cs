using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AgentBrowser.Config;

public static class SingBoxConfigBuilder
{
    public static bool TryBuildConfig(string input, ConnectionKind kind, out JsonObject config, out string kindLabel, out string error)
    {
        return kind switch
        {
            ConnectionKind.Vless => TryBuildVlessConfig(input, out config, out kindLabel, out error),
            ConnectionKind.Socks => TryBuildSocksConfig(input, out config, out kindLabel, out error),
            _ => FailBuild(out config, out kindLabel, out error)
        };
    }

    public static JsonSerializerOptions CreateIndentedJsonOptions()
    {
        return new JsonSerializerOptions { WriteIndented = true };
    }

    private static bool TryBuildVlessConfig(string input, out JsonObject config, out string kindLabel, out string error)
    {
        if (ConnectionParser.TryParseVless(input, out VlessConnection? vless, out error))
        {
            config = BuildVlessConfig(vless);
            kindLabel = "VLESS Reality/TLS";
            error = string.Empty;
            return true;
        }

        config = new JsonObject();
        kindLabel = "VLESS";
        return false;
    }

    private static bool TryBuildSocksConfig(string input, out JsonObject config, out string kindLabel, out string error)
    {
        if (ConnectionParser.TryParseSocks(input, out SocksConnection? socks, out error))
        {
            config = BuildSocksConfig(socks);
            kindLabel = "SOCKS5";
            error = string.Empty;
            return true;
        }

        config = new JsonObject();
        kindLabel = "SOCKS5";
        return false;
    }

    private static bool FailBuild(out JsonObject config, out string kindLabel, out string error)
    {
        config = new JsonObject();
        kindLabel = string.Empty;
        error = "Unsupported connection type.";
        return false;
    }

    private static JsonObject BuildSocksConfig(SocksConnection connection)
    {
        JsonObject outbound = new()
        {
            ["type"] = "socks",
            ["tag"] = "proxy",
            ["server"] = connection.Host,
            ["server_port"] = connection.Port
        };

        if (!string.IsNullOrWhiteSpace(connection.Username))
        {
            outbound["username"] = connection.Username;
            outbound["password"] = connection.Password;
        }

        return BuildBaseConfig(outbound);
    }

    private static JsonObject BuildVlessConfig(VlessConnection connection)
    {
        JsonObject tls = new()
        {
            ["enabled"] = true,
            ["server_name"] = connection.ServerName
        };

        if (!string.IsNullOrWhiteSpace(connection.Fingerprint))
        {
            tls["utls"] = new JsonObject
            {
                ["enabled"] = true,
                ["fingerprint"] = connection.Fingerprint
            };
        }

        if (string.Equals(connection.Security, "reality", StringComparison.OrdinalIgnoreCase))
        {
            tls["reality"] = new JsonObject
            {
                ["enabled"] = true,
                ["public_key"] = connection.PublicKey,
                ["short_id"] = connection.ShortId
            };
        }

        JsonObject outbound = new()
        {
            ["type"] = "vless",
            ["tag"] = "proxy",
            ["server"] = connection.Host,
            ["server_port"] = connection.Port,
            ["uuid"] = connection.Uuid,
            ["network"] = "tcp",
            ["tls"] = tls
        };

        if (!IPAddress.TryParse(connection.Host, out _))
        {
            outbound["domain_resolver"] = "local";
        }

        if (!string.IsNullOrWhiteSpace(connection.Flow))
        {
            outbound["flow"] = connection.Flow;
        }

        return BuildBaseConfig(outbound);
    }

    private static JsonObject BuildBaseConfig(JsonObject outbound)
    {
        return new JsonObject
        {
            ["log"] = new JsonObject
            {
                ["level"] = "warn",
                ["output"] = "sing-box.log",
                ["timestamp"] = true
            },
            ["dns"] = new JsonObject
            {
                ["servers"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["tag"] = "local",
                        ["type"] = "local"
                    },
                    new JsonObject
                    {
                        ["tag"] = "remote",
                        ["type"] = "tcp",
                        ["server"] = "1.1.1.1",
                        ["server_port"] = 53,
                        ["detour"] = "proxy"
                    }
                },
                ["final"] = "remote",
                ["strategy"] = "ipv4_only",
                ["independent_cache"] = true
            },
            ["inbounds"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "tun",
                    ["tag"] = "tun-in",
                    ["interface_name"] = "sb-tun",
                    ["address"] = new JsonArray("172.19.0.1/30", "fdfe:dcba:9876::1/126"),
                    ["mtu"] = 1500,
                    ["auto_route"] = true,
                    ["strict_route"] = true,
                    ["route_address"] = new JsonArray("0.0.0.0/1", "128.0.0.0/1", "::/1", "8000::/1"),
                    ["route_exclude_address"] = new JsonArray(
                        "127.0.0.0/8",
                        "10.0.0.0/8",
                        "172.16.0.0/12",
                        "192.168.0.0/16",
                        "169.254.0.0/16",
                        "224.0.0.0/4",
                        "::1/128",
                        "fc00::/7",
                        "fe80::/10"),
                    ["stack"] = "system"
                }
            },
            ["outbounds"] = new JsonArray
            {
                outbound,
                new JsonObject
                {
                    ["type"] = "direct",
                    ["tag"] = "direct"
                }
            },
            ["route"] = new JsonObject
            {
                ["rules"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["action"] = "sniff"
                    },
                    new JsonObject
                    {
                        ["protocol"] = "dns",
                        ["action"] = "hijack-dns"
                    },
                    new JsonObject
                    {
                        ["ip_is_private"] = true,
                        ["outbound"] = "direct"
                    }
                },
                ["auto_detect_interface"] = true,
                ["default_domain_resolver"] = "local",
                ["final"] = "proxy"
            }
        };
    }
}
