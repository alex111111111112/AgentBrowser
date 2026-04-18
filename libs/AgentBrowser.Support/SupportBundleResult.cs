namespace AgentBrowser.Support;

public sealed class SupportBundleResult
{
    public bool Success { get; init; }
    public string? BundlePath { get; init; }
    public int IncludedFileCount { get; init; }
    public string? ErrorMessage { get; init; }
}
