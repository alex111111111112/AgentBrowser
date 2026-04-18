using AgentBrowser.Workspaces;

namespace AgentBrowser.Sessions;

public static class SessionCoordinator
{
    public static SessionOperationResult RunStartup(string baseDir, string logPath)
    {
        return StartupFlow.Run(CreateContext(baseDir, logPath));
    }

    public static SessionOperationResult RunStop(string baseDir, string logPath)
    {
        return StopFlow.Run(CreateContext(baseDir, logPath));
    }

    private static SessionContext CreateContext(string baseDir, string logPath)
    {
        return new SessionContext
        {
            BaseDir = baseDir,
            LogPath = logPath,
            RuntimeLayout = WorkspaceRuntimeService.Discover(baseDir),
            WorkspacePaths = WorkspaceService.EnsureDefaultWorkspace(baseDir)
        };
    }
}
