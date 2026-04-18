using System.Net;

namespace AgentBrowser.Config;

public static class ConnectionParser
{
    public static ConnectionKind DetectConnectionKind(string input)
    {
        return input.TrimStart().StartsWith("vless://", StringComparison.OrdinalIgnoreCase)
            ? ConnectionKind.Vless
            : ConnectionKind.Socks;
    }

    public static ConnectionKind? TryParseConnectionKind(string? value)
    {
        return Enum.TryParse(value, ignoreCase: true, out ConnectionKind kind) ? kind : null;
    }

    public static bool TryParseVless(string input, out VlessConnection connection, out string error)
    {
        connection = null!;
        error = "Supported formats: vless://... or host:port:user:pass";

        if (!input.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Uri.TryCreate(input, UriKind.Absolute, out Uri? uri))
        {
            error = "Invalid VLESS URI.";
            return false;
        }

        string uuid = Uri.UnescapeDataString(uri.UserInfo ?? string.Empty);
        if (string.IsNullOrWhiteSpace(uuid))
        {
            error = "VLESS URI is missing the UUID.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(uri.Host) || uri.Port <= 0)
        {
            error = "VLESS URI is missing host or port.";
            return false;
        }

        Dictionary<string, string> query = ParseQueryString(uri.Query);
        string network = GetQueryValue(query, "type", "tcp");
        if (!string.Equals(network, "tcp", StringComparison.OrdinalIgnoreCase))
        {
            error = "This UI currently supports only VLESS TCP links.";
            return false;
        }

        string security = GetQueryValue(query, "security", "tls");
        string flow = GetQueryValue(query, "flow", string.Empty);
        string sni = GetQueryValue(query, "sni", uri.Host);
        string fingerprint = GetQueryValue(query, "fp", "chrome");
        string publicKey = GetQueryValue(query, "pbk", string.Empty);
        string shortId = GetQueryValue(query, "sid", string.Empty);

        if (string.Equals(security, "reality", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(publicKey))
            {
                error = "Reality VLESS URI is missing pbk.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(shortId))
            {
                error = "Reality VLESS URI is missing sid.";
                return false;
            }
        }
        else if (!string.Equals(security, "tls", StringComparison.OrdinalIgnoreCase))
        {
            error = $"Unsupported VLESS security mode: {security}";
            return false;
        }

        connection = new VlessConnection(
            uri.Host,
            uri.Port,
            uuid,
            flow,
            security,
            sni,
            fingerprint,
            publicKey,
            shortId);
        error = string.Empty;
        return true;
    }

    public static bool TryParseSocks(string input, out SocksConnection connection, out string error)
    {
        connection = null!;
        error = "SOCKS proxy must look like host:port or host:port:user:pass";

        if (input.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string[] parts = input.Split(':');
        if (parts.Length != 2 && parts.Length != 4)
        {
            return false;
        }

        string host = parts[0].Trim();
        if (string.IsNullOrWhiteSpace(host))
        {
            error = "SOCKS proxy is missing the host.";
            return false;
        }

        if (!int.TryParse(parts[1], out int port) || port <= 0 || port > 65535)
        {
            error = "SOCKS proxy has an invalid port.";
            return false;
        }

        string username = parts.Length == 4 ? parts[2] : string.Empty;
        string password = parts.Length == 4 ? parts[3] : string.Empty;

        connection = new SocksConnection(host, port, username, password);
        error = string.Empty;
        return true;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string trimmed = query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return values;
        }

        foreach (string pair in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = pair.Split('=', 2);
            string key = Uri.UnescapeDataString(parts[0]);
            string value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            values[key] = value;
        }

        return values;
    }

    private static string GetQueryValue(Dictionary<string, string> query, string key, string fallback)
    {
        return query.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }
}
