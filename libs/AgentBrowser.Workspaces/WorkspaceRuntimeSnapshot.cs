namespace AgentBrowser.Workspaces;

public sealed class WorkspaceRuntimeSnapshot
{
    public int BrowserProcessCount { get; init; }
    public int SingBoxProcessCount { get; init; }

    public bool IsBrowserRunning => BrowserProcessCount > 0;
    public bool IsSingBoxRunning => SingBoxProcessCount > 0;
}
