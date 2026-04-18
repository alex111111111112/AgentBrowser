using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

internal static class Program
{
    private const string XrayProcessName = "xray";
    private const string LogFileName = "stop_xray.log";

    [STAThread]
    private static void Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string xrayExe = Path.Combine(baseDir, "Xray", "xray.exe");
        string logPath = Path.Combine(baseDir, LogFileName);

        try
        {
            Log(logPath, "stop_xray started.");

            if (!File.Exists(xrayExe))
            {
                ShowError($"Missing file:{Environment.NewLine}{xrayExe}");
                Log(logPath, $"Missing file: {xrayExe}");
                return;
            }

            int stoppedCount = StopMatchingXrayProcesses(xrayExe, logPath);
            if (stoppedCount == 0)
            {
                Log(logPath, "No matching xray.exe process was running.");
            }
            else
            {
                Log(logPath, $"Stopped {stoppedCount} matching xray.exe process(es).");
            }
        }
        catch (Exception ex)
        {
            Log(logPath, "Unhandled exception: " + ex);
            ShowError("Failed to stop xray.exe." + Environment.NewLine + Environment.NewLine + ex.Message);
        }
    }

    private static int StopMatchingXrayProcesses(string expectedPath, string logPath)
    {
        string normalizedExpected = NormalizePath(expectedPath);
        int stoppedCount = 0;

        foreach (Process process in Process.GetProcessesByName(XrayProcessName))
        {
            try
            {
                string? processPath = process.MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(processPath))
                {
                    continue;
                }

                if (NormalizePath(processPath) != normalizedExpected)
                {
                    continue;
                }

                Log(logPath, $"Stopping PID={process.Id} from {processPath}.");
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
                stoppedCount++;
            }
            catch (Exception ex)
            {
                Log(logPath, $"Failed to stop PID={process.Id}: {ex.Message}");
                ShowError("Failed to stop xray.exe." + Environment.NewLine + Environment.NewLine + ex.Message);
                return stoppedCount;
            }
            finally
            {
                process.Dispose();
            }
        }

        return stoppedCount;
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Stop Xray",
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
            // Ignore logging failures.
        }
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .Trim()
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }
}
