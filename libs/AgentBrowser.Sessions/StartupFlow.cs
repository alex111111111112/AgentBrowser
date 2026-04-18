using System.Diagnostics;
using AgentBrowser.Workspaces;

namespace AgentBrowser.Sessions;

internal static class StartupFlow
{
    public static SessionOperationResult Run(SessionContext context)
    {
        Process? singBoxProcess = null;
        Mutex? instanceMutex = null;
        int? browserLaunchPid = null;

        try
        {
            SessionRuntimeHelpers.Log(context.LogPath, "Start.exe launched.");
            instanceMutex = SessionRuntimeHelpers.AcquireInstanceMutex(context.BaseDir, context.LogPath);
            if (instanceMutex is null)
            {
                return SessionOperationResult.Ok();
            }

            SessionLifecycleService.MarkStarting(context.WorkspacePaths, "Preparing browser session startup.", context.RuntimeLayout.SingBoxExecutablePath);

            SessionRuntimeHelpers.ValidateRequiredFile(context.RuntimeLayout.SingBoxExecutablePath, context.LogPath);
            SessionRuntimeHelpers.ValidateRequiredFile(context.RuntimeLayout.SingBoxConfigPath, context.LogPath);
            SessionRuntimeHelpers.ValidateRequiredFile(context.RuntimeLayout.SingBoxCronetPath, context.LogPath);
            SessionRuntimeHelpers.ValidateRequiredFile(context.RuntimeLayout.WintunDllPath, context.LogPath);
            SessionRuntimeHelpers.ValidateRequiredFile(context.RuntimeLayout.BrowserExecutablePath, context.LogPath);

            if (!SessionRuntimeHelpers.ValidateSingBoxConfig(
                    context.RuntimeLayout.CoreDir,
                    context.RuntimeLayout.SingBoxExecutablePath,
                    context.LogPath))
            {
                const string message = "sing-box configuration check failed. See start.log for details.";
                SessionLifecycleService.MarkError(context.WorkspacePaths, "sing-box configuration check failed.");
                return SessionRuntimeHelpers.Fail(context.LogPath, message);
            }

            Directory.CreateDirectory(context.RuntimeLayout.ProfileDir);

            HashSet<string> browserPaths = context.RuntimeLayout.BrowserPaths;
            if (browserPaths.Count == 0)
            {
                const string message = "Bundled browser executable was not found inside this folder.";
                return SessionRuntimeHelpers.Fail(context.LogPath, message);
            }

            WorkspaceRuntimeSnapshot initialSnapshot = WorkspaceRuntimeService.Inspect(context.RuntimeLayout);
            bool browserWasAlreadyRunning = initialSnapshot.IsBrowserRunning;
            bool singBoxWasAlreadyRunning = initialSnapshot.IsSingBoxRunning;

            SessionRuntimeHelpers.Log(context.LogPath, browserWasAlreadyRunning
                ? "Bundled browser is already running from this folder."
                : "Bundled browser is not running yet.");

            SessionRuntimeHelpers.Log(context.LogPath, singBoxWasAlreadyRunning
                ? "sing-box is already running from this folder. Skipping launch."
                : "sing-box is not running from this folder. Launching.");

            if (browserWasAlreadyRunning && singBoxWasAlreadyRunning)
            {
                SessionRuntimeHelpers.Log(context.LogPath, "Browser and sing-box are already active from this folder. Ignoring duplicate launch.");
                SessionLifecycleService.MarkRunning(
                    context.WorkspacePaths,
                    "Existing browser session is already active.",
                    singBoxExecutable: context.RuntimeLayout.SingBoxExecutablePath,
                    browserExecutable: context.RuntimeLayout.BrowserExecutablePath);
                return SessionOperationResult.Ok();
            }

            if (!singBoxWasAlreadyRunning)
            {
                singBoxProcess = SessionRuntimeHelpers.StartSingBoxProcess(
                    context.RuntimeLayout.CoreDir,
                    context.RuntimeLayout.SingBoxExecutablePath,
                    context.LogPath);
                Thread.Sleep(TimeSpan.FromSeconds(3));

                if (singBoxProcess is null)
                {
                    const string message = "sing-box did not start.";
                    SessionLifecycleService.MarkError(context.WorkspacePaths, "sing-box did not start.");
                    return SessionRuntimeHelpers.Fail(context.LogPath, message);
                }

                if (singBoxProcess.HasExited)
                {
                    SessionRuntimeHelpers.Log(context.LogPath, $"sing-box exited early. ExitCode={singBoxProcess.ExitCode}.");
                    SessionRuntimeHelpers.LogProcessOutput(context.LogPath, "sing-box stdout", singBoxProcess.StandardOutput);
                    SessionRuntimeHelpers.LogProcessOutput(context.LogPath, "sing-box stderr", singBoxProcess.StandardError);
                    const string message = "sing-box did not stay running after launch. See start.log for details.";
                    SessionLifecycleService.MarkError(context.WorkspacePaths, "sing-box did not stay running after launch.");
                    return SessionRuntimeHelpers.Fail(context.LogPath, message);
                }
            }

            bool singBoxIsRunning = WorkspaceRuntimeService.Inspect(context.RuntimeLayout).IsSingBoxRunning;
            SessionRuntimeHelpers.Log(context.LogPath, singBoxIsRunning
                ? "Verified sing-box is alive before browser launch."
                : "sing-box is not running before browser launch.");

            if (!singBoxIsRunning)
            {
                const string message = "sing-box did not stay running after launch. See start.log for details.";
                SessionLifecycleService.MarkError(context.WorkspacePaths, "sing-box did not stay running after launch.");
                return SessionRuntimeHelpers.Fail(context.LogPath, message);
            }

            if (browserWasAlreadyRunning)
            {
                SessionRuntimeHelpers.Log(context.LogPath, "Browser session already existed. Skipping browser launch and attaching to current session.");
                SessionLifecycleService.MarkRunning(
                    context.WorkspacePaths,
                    "Attached to existing browser session.",
                    singBoxPid: singBoxProcess?.Id,
                    singBoxExecutable: context.RuntimeLayout.SingBoxExecutablePath,
                    browserExecutable: context.RuntimeLayout.BrowserExecutablePath);
            }
            else
            {
                browserLaunchPid = SessionRuntimeHelpers.LaunchBrowserDirect(
                    context.RuntimeLayout.BrowserExecutablePath,
                    SessionRuntimeHelpers.BuildBrowserArguments(context.RuntimeLayout.ProfileDir),
                    context.BaseDir,
                    context.LogPath);

                if (!browserLaunchPid.HasValue)
                {
                    if (!singBoxWasAlreadyRunning)
                    {
                        SessionRuntimeHelpers.StopMatchingProcesses(
                            WorkspaceRuntimeService.SingBoxProcessName,
                            context.RuntimeLayout.SingBoxExecutablePath,
                            context.LogPath);
                    }

                    const string message = "Browser launch failed. See start.log for details.";
                    SessionLifecycleService.MarkError(context.WorkspacePaths, "Browser launch failed.");
                    return SessionRuntimeHelpers.Fail(context.LogPath, message);
                }
            }

            bool browserObserved = SessionRuntimeHelpers.WaitForBrowserAppearance(browserPaths, context.LogPath, TimeSpan.FromSeconds(45));
            if (!browserObserved)
            {
                SessionRuntimeHelpers.Log(context.LogPath, "Bundled browser process was not observed after launch request.");

                if (!singBoxWasAlreadyRunning)
                {
                    SessionRuntimeHelpers.StopMatchingProcesses(
                        WorkspaceRuntimeService.SingBoxProcessName,
                        context.RuntimeLayout.SingBoxExecutablePath,
                        context.LogPath);
                }

                const string message = "Browser did not appear after launch. See start.log for details.";
                SessionLifecycleService.MarkError(context.WorkspacePaths, "Browser did not appear after launch.");
                return SessionRuntimeHelpers.Fail(context.LogPath, message);
            }

            SessionLifecycleService.MarkRunning(
                context.WorkspacePaths,
                "Browser session is active.",
                singBoxPid: singBoxProcess?.Id,
                singBoxExecutable: context.RuntimeLayout.SingBoxExecutablePath,
                browserPid: browserLaunchPid,
                browserExecutable: context.RuntimeLayout.BrowserExecutablePath);
            SessionRuntimeHelpers.Log(context.LogPath, "Bundled browser session detected. Waiting for it to close.");
            SessionRuntimeHelpers.WaitForBrowserExit(browserPaths, context.LogPath);
            SessionRuntimeHelpers.Log(context.LogPath, "Bundled browser session ended.");
            SessionLifecycleService.MarkStopping(context.WorkspacePaths, "Browser session ended. Stopping tunnel.");

            if (!singBoxWasAlreadyRunning)
            {
                int stoppedCount = SessionRuntimeHelpers.StopMatchingProcesses(
                    WorkspaceRuntimeService.SingBoxProcessName,
                    context.RuntimeLayout.SingBoxExecutablePath,
                    context.LogPath);
                SessionRuntimeHelpers.Log(context.LogPath, stoppedCount == 0
                    ? "No matching sing-box process remained to stop."
                    : $"Stopped {stoppedCount} matching sing-box process(es).");
            }
            else
            {
                SessionRuntimeHelpers.Log(context.LogPath, "sing-box was already running before Start.exe. Leaving it running.");
            }

            SessionLifecycleService.MarkStopped(
                context.WorkspacePaths,
                singBoxWasAlreadyRunning
                    ? "Browser session ended. Existing sing-box process was left running."
                    : "Browser session ended and sing-box was stopped.");
            SessionRuntimeHelpers.Log(context.LogPath, "Start.exe finished.");
            return SessionOperationResult.Ok();
        }
        catch (FileNotFoundException ex)
        {
            SessionRuntimeHelpers.Log(context.LogPath, "Missing required file: " + ex.FileName);
            SessionLifecycleService.MarkError(context.WorkspacePaths, $"Missing required file: {ex.FileName}");
            return SessionOperationResult.Fail($"Missing file:{Environment.NewLine}{ex.FileName}");
        }
        catch (Exception ex)
        {
            SessionRuntimeHelpers.Log(context.LogPath, "Unhandled exception: " + ex);
            SessionLifecycleService.MarkError(context.WorkspacePaths, $"Unhandled start error: {ex.Message}");

            if (singBoxProcess is not null && !singBoxProcess.HasExited)
            {
                try
                {
                    singBoxProcess.Kill(entireProcessTree: true);
                    singBoxProcess.WaitForExit(5000);
                }
                catch
                {
                    // Ignore cleanup failures in the exception path.
                }
            }

            return SessionOperationResult.Fail("Start.exe failed." + Environment.NewLine + Environment.NewLine + ex.Message);
        }
        finally
        {
            singBoxProcess?.Dispose();
            if (instanceMutex is not null)
            {
                try
                {
                    instanceMutex.ReleaseMutex();
                }
                catch
                {
                    // Ignore mutex release failures.
                }

                instanceMutex.Dispose();
            }
        }
    }
}
