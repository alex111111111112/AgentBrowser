using AgentBrowser.Workspaces;

namespace AgentBrowser.Sessions;

internal static class StopFlow
{
    public static SessionOperationResult Run(SessionContext context)
    {
        try
        {
            SessionRuntimeHelpers.Log(context.LogPath, "Stop.exe launched.");
            SessionLifecycleService.MarkStopping(context.WorkspacePaths, "Stop.exe requested session shutdown.");

            int stoppedBrowserCount = SessionRuntimeHelpers.StopMatchingProcesses(
                WorkspaceRuntimeService.BrowserProcessName,
                context.RuntimeLayout.BrowserPaths,
                context.LogPath);
            SessionRuntimeHelpers.Log(context.LogPath, stoppedBrowserCount == 0
                ? "No matching bundled Chrome process was running."
                : $"Stopped {stoppedBrowserCount} matching bundled Chrome process(es).");

            int stoppedTunnelCount = 0;
            if (File.Exists(context.RuntimeLayout.SingBoxExecutablePath))
            {
                stoppedTunnelCount = SessionRuntimeHelpers.StopMatchingProcesses(
                    WorkspaceRuntimeService.SingBoxProcessName,
                    context.RuntimeLayout.SingBoxExecutablePath,
                    context.LogPath);
                SessionRuntimeHelpers.Log(context.LogPath, stoppedTunnelCount == 0
                    ? "No matching sing-box process was running."
                    : $"Stopped {stoppedTunnelCount} matching sing-box process(es).");
            }
            else
            {
                SessionRuntimeHelpers.Log(context.LogPath, $"sing-box executable is missing: {context.RuntimeLayout.SingBoxExecutablePath}");
            }

            if (stoppedBrowserCount == 0 && stoppedTunnelCount == 0)
            {
                SessionRuntimeHelpers.Log(context.LogPath, "Nothing from this package was running.");
            }

            SessionLifecycleService.MarkStopped(
                context.WorkspacePaths,
                stoppedBrowserCount == 0 && stoppedTunnelCount == 0
                    ? "Stop.exe found no running browser session."
                    : "Stop.exe stopped the active browser session.");

            return SessionOperationResult.Ok();
        }
        catch (Exception ex)
        {
            SessionRuntimeHelpers.Log(context.LogPath, "Unhandled exception: " + ex);
            SessionLifecycleService.MarkError(context.WorkspacePaths, $"Stop.exe failed: {ex.Message}");
            return SessionOperationResult.Fail("Failed to stop Agent Browser." + Environment.NewLine + Environment.NewLine + ex.Message);
        }
    }
}
