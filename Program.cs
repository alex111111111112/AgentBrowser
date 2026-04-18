using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    private const string XrayProcessName = "xray";
    private const string LogFileName = "launcher.log";

    [STAThread]
    private static void Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string xrayExe = Path.Combine(baseDir, "Xray", "xray.exe");
        string xrayConfig = Path.Combine(baseDir, "Xray", "config.json");
        string firefoxExe = Path.Combine(baseDir, "FirefoxPortable.exe");
        string logPath = Path.Combine(baseDir, LogFileName);

        try
        {
            Log(logPath, "Launcher started.");

            if (!File.Exists(xrayExe))
            {
                Fail(logPath, $"Missing file: {xrayExe}");
                return;
            }

            if (!File.Exists(xrayConfig))
            {
                Fail(logPath, $"Missing file: {xrayConfig}");
                return;
            }

            if (!File.Exists(firefoxExe))
            {
                Fail(logPath, $"Missing file: {firefoxExe}");
                return;
            }

            bool xrayWasAlreadyRunning = IsXrayAlreadyRunningFromPath(xrayExe);
            Log(logPath, xrayWasAlreadyRunning
                ? "xray.exe is already running from this folder. Skipping launch."
                : "xray.exe is not running from this folder. Launching.");

            if (!xrayWasAlreadyRunning)
            {
                var xrayStartInfo = new ProcessStartInfo
                {
                    FileName = xrayExe,
                    Arguments = $"-config \"{xrayConfig}\"",
                    WorkingDirectory = Path.GetDirectoryName(xrayExe) ?? baseDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using Process? xrayProcess = Process.Start(xrayStartInfo);
                Log(logPath, xrayProcess is null
                    ? "Process.Start returned null for xray.exe."
                    : $"xray.exe start requested. PID={xrayProcess.Id}.");

                Thread.Sleep(2000);

                if (xrayProcess is not null && xrayProcess.HasExited)
                {
                    Log(logPath, $"xray.exe exited early. ExitCode={xrayProcess.ExitCode}.");

                    string stdout = SafeReadToEnd(xrayProcess.StandardOutput);
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        Log(logPath, "xray stdout: " + stdout);
                    }

                    string stderr = SafeReadToEnd(xrayProcess.StandardError);
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        Log(logPath, "xray stderr: " + stderr);
                    }
                }
            }

            bool xrayIsRunning = IsXrayAlreadyRunningFromPath(xrayExe);
            Log(logPath, xrayIsRunning
                ? "Verified xray.exe is alive before launcher exit."
                : "xray.exe is not running before launcher exit.");

            if (!xrayIsRunning)
            {
                ShowError("xray.exe did not stay running after launch. See launcher.log for details.");
                return;
            }

            var firefoxStartInfo = new ProcessStartInfo
            {
                FileName = firefoxExe,
                WorkingDirectory = baseDir,
                UseShellExecute = true
            };

            using Process? firefoxProcess = Process.Start(firefoxStartInfo);
            Log(logPath, firefoxProcess is null
                ? "Process.Start returned null for FirefoxPortable.exe."
                : $"FirefoxPortable.exe start requested. PID={firefoxProcess.Id}.");

            Log(logPath, "Launcher finished successfully.");
        }
        catch (Exception ex)
        {
            Log(logPath, "Unhandled exception: " + ex);
            ShowError("Launcher error:\n\n" + ex.Message);
        }
    }

    private static void Fail(string logPath, string message)
    {
        Log(logPath, message);
        ShowError(message);
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Launcher error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private static void Log(string logPath, string message)
    {
        try
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(logPath, line);
        }
        catch
        {
            // Ignore logging failures to avoid blocking launcher behavior.
        }
    }

    private static bool IsXrayAlreadyRunningFromPath(string expectedPath)
    {
        string normalizedExpected = NormalizePath(expectedPath);

        foreach (Process process in Process.GetProcessesByName(XrayProcessName))
        {
            try
            {
                string? processPath = process.MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(processPath))
                {
                    continue;
                }

                if (NormalizePath(processPath) == normalizedExpected)
                {
                    return true;
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

        return false;
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .Trim()
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    private static string SafeReadToEnd(StreamReader reader)
    {
        try
        {
            return reader.ReadToEnd().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
}
