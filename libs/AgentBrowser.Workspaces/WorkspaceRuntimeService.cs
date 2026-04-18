using System.Diagnostics;

namespace AgentBrowser.Workspaces;

public static class WorkspaceRuntimeService
{
    public const string BrowserProcessName = "chrome";
    public const string SingBoxProcessName = "sing-box";

    public static WorkspaceRuntimeLayout Discover(string baseDir)
    {
        string coreDir = Path.Combine(baseDir, "core");
        string browserExecutablePath = ResolveBrowserExecutable(baseDir);

        return new WorkspaceRuntimeLayout
        {
            BaseDir = baseDir,
            CoreDir = coreDir,
            ProfileDir = Path.Combine(baseDir, "Profile"),
            SingBoxExecutablePath = Path.Combine(coreDir, "sing-box.exe"),
            SingBoxConfigPath = Path.Combine(coreDir, "config.json"),
            SingBoxCronetPath = Path.Combine(coreDir, "libcronet.dll"),
            WintunDllPath = Path.Combine(coreDir, "wintun.dll"),
            BrowserExecutablePath = browserExecutablePath,
            BrowserPaths = GetBundledBrowserPaths(baseDir)
        };
    }

    public static WorkspaceRuntimeSnapshot Inspect(WorkspaceRuntimeLayout layout)
    {
        return new WorkspaceRuntimeSnapshot
        {
            BrowserProcessCount = CountMatchingProcesses(BrowserProcessName, layout.BrowserPaths),
            SingBoxProcessCount = CountMatchingProcesses(SingBoxProcessName, layout.SingBoxExecutablePath)
        };
    }

    public static int CountMatchingProcesses(string processName, string expectedPath)
    {
        string normalizedExpected = NormalizePath(expectedPath);
        int count = 0;

        foreach (Process process in Process.GetProcessesByName(processName))
        {
            try
            {
                if (!TryGetProcessPath(process, out string processPath))
                {
                    continue;
                }

                if (string.Equals(NormalizePath(processPath), normalizedExpected, StringComparison.Ordinal))
                {
                    count++;
                }
            }
            catch
            {
                // Ignore access-denied and transient process inspection failures.
            }
            finally
            {
                process.Dispose();
            }
        }

        return count;
    }

    public static int CountMatchingProcesses(string processName, HashSet<string> expectedPaths)
    {
        int count = 0;

        foreach (Process process in Process.GetProcessesByName(processName))
        {
            try
            {
                if (!TryGetProcessPath(process, out string processPath))
                {
                    continue;
                }

                if (expectedPaths.Contains(NormalizePath(processPath)))
                {
                    count++;
                }
            }
            catch
            {
                // Ignore access-denied and transient process inspection failures.
            }
            finally
            {
                process.Dispose();
            }
        }

        return count;
    }

    public static bool TryGetProcessPath(Process process, out string processPath)
    {
        processPath = string.Empty;

        try
        {
            string? candidatePath = process.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(candidatePath))
            {
                return false;
            }

            processPath = candidatePath;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .Trim()
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    private static string ResolveBrowserExecutable(string baseDir)
    {
        string directBrowser = Path.Combine(baseDir, "App", "Chrome-bin", "chrome.exe");
        if (File.Exists(directBrowser))
        {
            return directBrowser;
        }

        return Path.Combine(baseDir, "chrome.exe");
    }

    private static HashSet<string> GetBundledBrowserPaths(string baseDir)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string directBrowser = Path.Combine(baseDir, "App", "Chrome-bin", "chrome.exe");
        if (File.Exists(directBrowser))
        {
            paths.Add(NormalizePath(directBrowser));
        }

        string launcherBrowser = Path.Combine(baseDir, "chrome.exe");
        if (File.Exists(launcherBrowser))
        {
            paths.Add(NormalizePath(launcherBrowser));
        }

        string chromeBinDir = Path.Combine(baseDir, "App", "Chrome-bin");
        if (Directory.Exists(chromeBinDir))
        {
            foreach (string path in Directory.EnumerateFiles(chromeBinDir, "chrome.exe", SearchOption.AllDirectories))
            {
                paths.Add(NormalizePath(path));
            }
        }

        return paths;
    }
}
