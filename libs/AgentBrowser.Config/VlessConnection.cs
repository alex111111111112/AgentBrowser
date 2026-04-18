namespace AgentBrowser.Config;

public sealed record VlessConnection(
    string Host,
    int Port,
    string Uuid,
    string Flow,
    string Security,
    string ServerName,
    string Fingerprint,
    string PublicKey,
    string ShortId);
