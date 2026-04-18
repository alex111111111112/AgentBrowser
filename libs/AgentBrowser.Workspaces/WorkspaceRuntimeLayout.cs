namespace AgentBrowser.Workspaces;

public sealed class WorkspaceRuntimeLayout
{
    public required string BaseDir { get; init; }
    public required string CoreDir { get; init; }
    public required string ProfileDir { get; init; }
    public required string SingBoxExecutablePath { get; init; }
    public required string SingBoxConfigPath { get; init; }
    public required string SingBoxCronetPath { get; init; }
    public required string WintunDllPath { get; init; }
    public required string BrowserExecutablePath { get; init; }
    public required HashSet<string> BrowserPaths { get; init; }
}
