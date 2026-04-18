namespace AgentBrowser.Support;

public sealed class SupportBundleManifest
{
    public required DateTime CreatedUtc { get; init; }
    public required string WorkspaceId { get; init; }
    public required string SourceBaseDir { get; init; }
    public required List<string> IncludedFiles { get; init; }
}
