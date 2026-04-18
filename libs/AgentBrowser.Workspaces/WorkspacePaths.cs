namespace AgentBrowser.Workspaces;

public sealed class WorkspacePaths
{
    public WorkspacePaths(string baseDir, string workspaceId)
    {
        BaseDir = baseDir;
        WorkspaceId = workspaceId;
        WorkspacesRootDir = Path.Combine(baseDir, "workspaces");
        WorkspaceRootDir = Path.Combine(WorkspacesRootDir, workspaceId);
        ManifestPath = Path.Combine(WorkspaceRootDir, "manifest.json");
        SessionStatusPath = Path.Combine(WorkspaceRootDir, "session-status.json");
        WorkspaceSettingsPath = Path.Combine(WorkspaceRootDir, "ui-settings.json");
        LegacySettingsPath = Path.Combine(baseDir, "ui-settings.json");
    }

    public string BaseDir { get; }
    public string WorkspaceId { get; }
    public string WorkspacesRootDir { get; }
    public string WorkspaceRootDir { get; }
    public string ManifestPath { get; }
    public string SessionStatusPath { get; }
    public string WorkspaceSettingsPath { get; }
    public string LegacySettingsPath { get; }
}
