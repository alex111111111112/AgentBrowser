using System.Runtime.InteropServices;
using AgentBrowser.Sessions;

internal static class Program
{
    private const string LogFileName = "start.log";
    private const uint MbIconError = 0x00000010;

    [STAThread]
    private static void Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string logPath = Path.Combine(baseDir, LogFileName);

        SessionOperationResult result = SessionCoordinator.RunStartup(baseDir, logPath);
        if (!result.Success && !string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            ShowError(result.ErrorMessage);
        }
    }

    private static void ShowError(string message)
    {
        MessageBoxW(IntPtr.Zero, message, "Agent Browser", MbIconError);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
