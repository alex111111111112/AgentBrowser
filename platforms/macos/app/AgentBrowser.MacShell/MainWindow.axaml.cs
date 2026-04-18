using Avalonia.Controls;

namespace AgentBrowser.MacShell;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ShellSummary.Create();
    }
}
