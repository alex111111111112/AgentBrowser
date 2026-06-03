using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using AgentBrowser.Config;
using AgentBrowser.Diagnostics;
using AgentBrowser.Workspaces;

namespace AgentBrowser.Sessions;

internal static class SessionRuntimeHelpers
{
    public static void ValidateRequiredFile(string path, string logPath)
    {
        if (!File.Exists(path))
        {
            Log(logPath, $"Missing file: {path}");
            throw new FileNotFoundException("Missing required file.", path);
        }
    }

    public static Process? StartSingBoxProcess(string coreDir, string singBoxExe, string logPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = singBoxExe,
            Arguments = "-D . -C . run",
            WorkingDirectory = coreDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        Process? process = Process.Start(startInfo);
        Log(logPath, process is null
            ? "Process.Start returned null for sing-box."
            : $"sing-box start requested. PID={process.Id}.");

        return process;
    }

    public static bool ValidateSingBoxConfig(string coreDir, string singBoxExe, string logPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = singBoxExe,
            Arguments = "-D . -C . check",
            WorkingDirectory = coreDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            Log(logPath, "Process.Start returned null for sing-box check.");
            return false;
        }

        Log(logPath, $"sing-box config check started. PID={process.Id}.");
        process.WaitForExit(15000);

        if (!process.HasExited)
        {
            Log(logPath, "sing-box config check timed out.");

            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
            catch
            {
                // Ignore cleanup failures for the config check helper.
            }

            return false;
        }

        Log(logPath, $"sing-box config check exit code: {process.ExitCode}.");
        if (process.ExitCode != 0)
        {
            LogProcessOutput(logPath, "sing-box check stdout", process.StandardOutput);
            LogProcessOutput(logPath, "sing-box check stderr", process.StandardError);
            return false;
        }

        return true;
    }

    public static int? LaunchBrowserDirect(string browserExe, string arguments, string baseDir, string logPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = browserExe,
            Arguments = arguments,
            WorkingDirectory = baseDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            Log(logPath, "Process.Start returned null for browser.");
            return null;
        }

        Log(logPath, $"Browser start requested. PID={process.Id}. Executable={browserExe}");
        return process.Id;
    }

    public static string BuildBrowserArguments(string profileDir, RuntimeMode runtimeMode)
    {
        var arguments = new List<string>
        {
            $"--user-data-dir=\"{profileDir}\"",
            "--no-first-run",
            "--force-webrtc-ip-handling-policy=disable_non_proxied_udp"
        };

        if (runtimeMode == RuntimeMode.BrowserProxy)
        {
            arguments.Add($"--proxy-server=socks5://{SingBoxConfigBuilder.LocalSocksListenAddress}:{SingBoxConfigBuilder.LocalSocksListenPort}");
        }

        return string.Join(
            " ",
            arguments);
    }

    public static bool WaitForBrowserAppearance(HashSet<string> browserPaths, string logPath, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < deadline)
        {
            int count = WorkspaceRuntimeService.CountMatchingProcesses(WorkspaceRuntimeService.BrowserProcessName, browserPaths);
            if (count > 0)
            {
                Log(logPath, $"Observed {count} bundled browser process(es).");
                return true;
            }

            Thread.Sleep(1000);
        }

        return false;
    }

    public static Mutex? AcquireInstanceMutex(string baseDir, string logPath)
    {
        string normalizedBaseDir = WorkspaceRuntimeService.NormalizePath(baseDir);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedBaseDir)));
        Mutex mutex = new(initiallyOwned: true, name: @"Local\AgentBrowserStart_" + hash, createdNew: out bool createdNew);

        if (createdNew)
        {
            Log(logPath, "Start.exe instance lock acquired.");
            return mutex;
        }

        Log(logPath, "Another Start.exe instance is already active. Ignoring duplicate launch.");
        mutex.Dispose();
        return null;
    }

    public static void WaitForBrowserExit(HashSet<string> browserPaths, string logPath)
    {
        int consecutiveZeroCounts = 0;
        int lastCount = -1;

        while (consecutiveZeroCounts < 3)
        {
            int count = WorkspaceRuntimeService.CountMatchingProcesses(WorkspaceRuntimeService.BrowserProcessName, browserPaths);
            if (count != lastCount)
            {
                Log(logPath, $"Bundled browser process count: {count}.");
                lastCount = count;
            }

            consecutiveZeroCounts = count == 0 ? consecutiveZeroCounts + 1 : 0;
            Thread.Sleep(2000);
        }
    }

    public static int StopMatchingProcesses(string processName, string expectedPath, string logPath)
    {
        string normalizedExpected = WorkspaceRuntimeService.NormalizePath(expectedPath);
        int stoppedCount = 0;

        foreach (Process process in Process.GetProcessesByName(processName).OrderBy(p => p.Id))
        {
            try
            {
                if (!TryGetProcessPath(process, processName, logPath, out string processPath))
                {
                    continue;
                }

                if (!string.Equals(WorkspaceRuntimeService.NormalizePath(processPath), normalizedExpected, StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryStopProcess(process, processName, processPath, logPath))
                {
                    stoppedCount++;
                }
            }
            finally
            {
                process.Dispose();
            }
        }

        return stoppedCount;
    }

    public static int StopMatchingProcesses(string processName, HashSet<string> expectedPaths, string logPath)
    {
        int stoppedCount = 0;

        foreach (Process process in Process.GetProcessesByName(processName).OrderBy(p => p.Id))
        {
            try
            {
                if (!TryGetProcessPath(process, processName, logPath, out string processPath))
                {
                    continue;
                }

                if (!expectedPaths.Contains(WorkspaceRuntimeService.NormalizePath(processPath)))
                {
                    continue;
                }

                if (TryStopProcess(process, processName, processPath, logPath))
                {
                    stoppedCount++;
                }
            }
            finally
            {
                process.Dispose();
            }
        }

        return stoppedCount;
    }

    public static bool TryGetProcessPath(Process process, string processName, string logPath, out string processPath)
    {
        processPath = string.Empty;

        try
        {
            if (!WorkspaceRuntimeService.TryGetProcessPath(process, out string candidatePath))
            {
                Log(logPath, $"Skipping {processName} PID={process.Id}: executable path is unavailable.");
                return false;
            }

            processPath = candidatePath;
            return true;
        }
        catch (Exception ex)
        {
            Log(logPath, $"Skipping {processName} PID={process.Id}: failed to inspect path. {ex.Message}");
            return false;
        }
    }

    public static bool TryStopProcess(Process process, string processName, string processPath, string logPath)
    {
        try
        {
            Log(logPath, $"Stopping {processName} PID={process.Id} from {processPath}.");
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            Log(logPath, $"{processName} PID={process.Id} had already exited.");
            return false;
        }
        catch (Exception ex)
        {
            Log(logPath, $"Failed to send kill to {processName} PID={process.Id}: {ex.Message}");
            return false;
        }

        try
        {
            process.WaitForExit(5000);
            if (process.HasExited)
            {
                return true;
            }

            Thread.Sleep(500);
            return process.HasExited;
        }
        catch (Exception ex)
        {
            Log(logPath, $"Stop wait failed for {processName} PID={process.Id}: {ex.Message}");
            return process.HasExited;
        }
    }

    public static void LogProcessOutput(string logPath, string prefix, StreamReader reader)
    {
        try
        {
            string output = reader.ReadToEnd().Trim();
            if (!string.IsNullOrWhiteSpace(output))
            {
                Log(logPath, $"{prefix}: {output}");
            }
        }
        catch
        {
            // Ignore output capture failures.
        }
    }

    public static SessionOperationResult Fail(string logPath, string message)
    {
        Log(logPath, message);
        return SessionOperationResult.Fail(message);
    }

    public static void Log(string logPath, string message)
    {
        TimestampedFileLog.Write(logPath, message);
    }
}
