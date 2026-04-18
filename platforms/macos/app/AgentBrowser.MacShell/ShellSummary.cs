using AgentBrowser.Diagnostics;
using AgentBrowser.Workspaces;

namespace AgentBrowser.MacShell;

internal sealed class ShellSummary
{
    public string WindowTitle { get; private init; } = "AgentBrowser macOS Shell";
    public string Heading { get; private init; } = string.Empty;
    public string Subheading { get; private init; } = string.Empty;
    public string ShellStatus { get; private init; } = string.Empty;
    public string PlatformName { get; private init; } = string.Empty;
    public string BundleBaseDir { get; private init; } = string.Empty;
    public string AppSupportRoot { get; private init; } = string.Empty;
    public string WorkspaceRoot { get; private init; } = string.Empty;
    public string ManifestPath { get; private init; } = string.Empty;
    public string SessionStatusPath { get; private init; } = string.Empty;
    public string LogPath { get; private init; } = string.Empty;
    public string SharedLibraries { get; private init; } = string.Empty;
    public string RuntimeBoundary { get; private init; } = string.Empty;
    public string NextSteps { get; private init; } = string.Empty;

    public static ShellSummary Create()
    {
        string appSupportRoot = MacShellPaths.ResolveAppSupportRoot();
        Directory.CreateDirectory(appSupportRoot);

        string logPath = MacShellPaths.ResolveLogPath(appSupportRoot);
        TimestampedFileLog.Write(logPath, "AgentBrowser.MacShell initialized.");

        WorkspacePaths workspace = WorkspaceService.EnsureDefaultWorkspace(appSupportRoot);

        return new ShellSummary
        {
            Heading = "Experimental macOS operator shell",
            Subheading = "This is the first macOS shell spike. It proves desktop shell structure and shared-library reuse without trying to ship tunnel parity yet.",
            ShellStatus = "Ready for UI-shell experiments",
            PlatformName = MacShellPaths.DescribePlatform(),
            BundleBaseDir = AppContext.BaseDirectory,
            AppSupportRoot = appSupportRoot,
            WorkspaceRoot = workspace.WorkspaceRootDir,
            ManifestPath = workspace.ManifestPath,
            SessionStatusPath = workspace.SessionStatusPath,
            LogPath = logPath,
            SharedLibraries = "AgentBrowser.Workspaces + AgentBrowser.Diagnostics",
            RuntimeBoundary = "No tunnel orchestration, privileges, entitlements, or packaging parity in this spike.",
            NextSteps = string.Join(
                Environment.NewLine,
                "1. Keep the shell lightweight and operator-oriented.",
                "2. Decide whether presets should reuse current config generation as-is.",
                "3. Design a macOS runtime seam under platforms/macos/runtime/.",
                "4. Add packaging/signing only after the shell direction is validated.")
        };
    }
}
