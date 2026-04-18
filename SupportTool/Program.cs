using System.Diagnostics;
using AgentBrowser.Diagnostics;
using AgentBrowser.Support;

Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
Application.ThreadException += (_, e) =>
{
    WriteLog("Unhandled support tool UI exception: " + e.Exception);
    MessageBox.Show(
        "SupportTool failed. See support-tool.log for details.",
        "Support Tool",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
};

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    WriteLog("Unhandled support tool domain exception: " + e.ExceptionObject);
};

try
{
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string supportDir = Path.Combine(baseDir, "support");
    string defaultFileName = $"AgentBrowser_Support_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

    WriteLog("SupportTool.exe launched.");

    using var dialog = new SaveFileDialog
    {
        Title = "Export Support Bundle",
        InitialDirectory = supportDir,
        FileName = defaultFileName,
        Filter = "Zip archive (*.zip)|*.zip",
        DefaultExt = "zip",
        AddExtension = true,
        OverwritePrompt = true
    };

    if (dialog.ShowDialog() != DialogResult.OK)
    {
        WriteLog("Support bundle export canceled by user.");
        return;
    }

    WriteLog($"Creating support bundle: {dialog.FileName}");
    SupportBundleResult result = SupportBundleService.CreateBundle(baseDir, dialog.FileName);
    if (!result.Success)
    {
        string message = result.ErrorMessage ?? "Support bundle export failed.";
        WriteLog($"Support bundle export failed: {message}");
        MessageBox.Show(
            message,
            "Support Tool",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        return;
    }

    WriteLog($"Support bundle exported: {result.BundlePath} ({result.IncludedFileCount} files)");
    DialogResult openFolder = MessageBox.Show(
        $"Support bundle exported:{Environment.NewLine}{result.BundlePath}{Environment.NewLine}{Environment.NewLine}Included files: {result.IncludedFileCount}{Environment.NewLine}{Environment.NewLine}Open the folder now?",
        "Support Tool",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Information);

    if (openFolder == DialogResult.Yes && !string.IsNullOrWhiteSpace(result.BundlePath))
    {
        string? folder = Path.GetDirectoryName(result.BundlePath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
    }
}
catch (Exception ex)
{
    WriteLog("Fatal support tool exception: " + ex);
    MessageBox.Show(
        "SupportTool failed. See support-tool.log for details.",
        "Support Tool",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}

static void WriteLog(string message)
{
    try
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "support-tool.log");
        TimestampedFileLog.Write(path, message);
    }
    catch
    {
        // Ignore logging failures.
    }
}
