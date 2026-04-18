using AgentBrowser.Workspaces;

namespace AgentBrowser.Sessions;

internal sealed class SessionContext
{
    public required string BaseDir { get; init; }
    public required string LogPath { get; init; }
    public required WorkspaceRuntimeLayout RuntimeLayout { get; init; }
    public required WorkspacePaths WorkspacePaths { get; init; }
}
