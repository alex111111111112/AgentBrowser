namespace AgentBrowser.Workspaces;

public sealed class WorkspaceManifest
{
    public string Id { get; set; } = "default";
    public string Name { get; set; } = "Default Workspace";
    public int SchemaVersion { get; set; } = 1;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
