using System.Runtime.InteropServices;

namespace AgentBrowser.MacShell;

internal static class MacShellPaths
{
    public static string ResolveAppSupportRoot()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(home))
        {
            return Path.Combine(AppContext.BaseDirectory, "macos-shell-data");
        }

        // Prefer the conventional macOS writable app-support area over bundle-relative paths.
        return Path.Combine(home, "Library", "Application Support", "AgentBrowser", "macos-shell");
    }

    public static string ResolveLogPath(string appSupportRoot)
    {
        return Path.Combine(appSupportRoot, "macos-shell.log");
    }

    public static string DescribePlatform()
    {
        return $"{RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture})";
    }
}
