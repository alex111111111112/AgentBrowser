using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using AgentBrowser.Config;
using AgentBrowser.Diagnostics;
using AgentBrowser.Support;
using AgentBrowser.Workspaces;

Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
Application.ThreadException += (_, e) =>
{
    UiLog.WriteException("Unhandled UI exception", e.Exception);
    MessageBox.Show(
        UiText.Text("error.unexpected_ui"),
        UiText.Text("app.title"),
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
};
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    if (e.ExceptionObject is Exception exception)
    {
        UiLog.WriteException("Unhandled domain exception", exception);
    }
    else
    {
        UiLog.Write($"Unhandled domain exception: {e.ExceptionObject}");
    }
};

try
{
    UiLog.Write("AgentBrowserUI.exe launched.");
    ApplicationConfiguration.Initialize();
    Application.Run(new MainForm());
    UiLog.Write("AgentBrowserUI.exe finished.");
}
catch (Exception ex)
{
    UiLog.WriteException("Fatal startup exception", ex);
    MessageBox.Show(
        UiText.Text("error.fatal_startup"),
        UiText.Text("app.title"),
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}

internal static class UiLog
{
    private static readonly object Sync = new();

    private static string LogPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui.log");

    public static void Write(string message)
    {
        try
        {
            lock (Sync)
            {
                TimestampedFileLog.Write(LogPath, message);
            }
        }
        catch
        {
            // Ignore logging failures.
        }
    }

    public static void WriteException(string context, Exception ex)
    {
        TimestampedFileLog.WriteException(LogPath, context, ex);
    }
}

internal sealed class MainForm : Form
{
    private const string StartExeName = "Start.exe";
    private const string StopExeName = "Stop.exe";
    private const string ProModeFlagFileName = "pro-mode.flag";
    private const string ProModePasswordEnvVar = "AGENT_BROWSER_PRO_PASSWORD";

    private readonly Label presetValueLabel;
    private readonly Label summaryValueLabel;
    private readonly Label runtimeStatusValueLabel;
    private readonly Label connectionHealthLampLabel;
    private readonly Label connectionHealthValueLabel;
    private readonly Label statusLabel;
    private readonly Button testButton;
    private readonly Button startButton;
    private readonly Button stopButton;
    private readonly Button settingsButton;
    private readonly Button exportSupportButton;
    private readonly Button openLogsButton;
    private readonly System.Windows.Forms.Timer statusTimer;
    private readonly WorkspacePaths workspacePaths;

    private static readonly HttpClient ConnectivityHttpClient = CreateConnectivityHttpClient();
    private static readonly TimeSpan ConnectivityRefreshInterval = TimeSpan.FromSeconds(10);

    private SettingsData settingsData = new();
    private bool proModeUnlockedForSession;
    private bool connectivityCheckInProgress;
    private DateTime lastConnectivityCheckUtc = DateTime.MinValue;
    private SessionState lastEffectiveState = SessionState.Stopped;
    private ConnectivityState connectivityState = ConnectivityState.Idle;
    private string connectivityStatusText = UiText.Text("connection.tunnel_stopped");

    private string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
    private string ConfigPath => Path.Combine(BaseDir, "core", "config.json");

    public MainForm()
    {
        workspacePaths = WorkspaceService.EnsureDefaultWorkspace(AppDomain.CurrentDomain.BaseDirectory);

        Text = UiText.Text("app.title");
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        ClientSize = new Size(920, 320);

        var titleLabel = new Label
        {
            AutoSize = true,
            Left = 16,
            Top = 16,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Text = UiText.Text("main.title")
        };

        var hintLabel = new Label
        {
            Left = 16,
            Top = 48,
            Width = 720,
            Height = 38,
            Text = UiText.Text("main.hint")
        };

        var presetCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 16,
            Top = 92,
            Text = UiText.Text("main.current_preset")
        };

        presetValueLabel = new Label
        {
            Left = 16,
            Top = 112,
            Width = 720,
            Height = 24,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Text = UiText.Text("main.no_preset")
        };

        var summaryCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 16,
            Top = 148,
            Text = UiText.Text("main.connection_summary")
        };

        summaryValueLabel = new Label
        {
            Left = 16,
            Top = 168,
            Width = 720,
            Height = 44,
            Font = new Font("Consolas", 9F),
            Text = UiText.Text("main.no_summary")
        };

        var runtimeStatusCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 756,
            Top = 92,
            Text = UiText.Text("main.status")
        };

        runtimeStatusValueLabel = new Label
        {
            Left = 756,
            Top = 112,
            Width = 148,
            Height = 24,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Text = UiText.FormatSessionState(SessionState.Stopped),
            ForeColor = Color.DarkRed
        };

        var connectionHealthCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 756,
            Top = 148,
            Text = UiText.Text("main.connection")
        };

        connectionHealthLampLabel = new Label
        {
            Left = 756,
            Top = 165,
            Width = 16,
            Height = 20,
            Text = "●",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.Gray
        };

        connectionHealthValueLabel = new Label
        {
            Left = 776,
            Top = 164,
            Width = 128,
            Height = 40,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Text = UiText.Text("connection.tunnel_stopped")
        };

        statusLabel = new Label
        {
            Left = 16,
            Top = 224,
            Width = 888,
            Height = 32,
            Text = UiText.Text("main.ready")
        };

        testButton = new Button
        {
            Left = 16,
            Top = 272,
            Width = 120,
            Height = 32,
            Text = UiText.Text("button.test")
        };
        testButton.Click += (_, _) => TestSelectedPreset();

        startButton = new Button
        {
            Left = 146,
            Top = 272,
            Width = 120,
            Height = 32,
            Text = UiText.Text("button.start")
        };
        startButton.Click += (_, _) => StartSelectedPreset();

        stopButton = new Button
        {
            Left = 276,
            Top = 272,
            Width = 120,
            Height = 32,
            Text = UiText.Text("button.stop")
        };
        stopButton.Click += (_, _) => LaunchHelper(StopExeName);

        settingsButton = new Button
        {
            Left = 406,
            Top = 272,
            Width = 120,
            Height = 32,
            Text = UiText.Text("button.settings")
        };
        settingsButton.Click += (_, _) => OpenSettings();

        exportSupportButton = new Button
        {
            Left = 536,
            Top = 272,
            Width = 150,
            Height = 32,
            Text = UiText.Text("button.export_bundle")
        };
        exportSupportButton.Click += (_, _) => ExportSupportBundle();

        openLogsButton = new Button
        {
            Left = 694,
            Top = 272,
            Width = 110,
            Height = 32,
            Text = UiText.Text("button.open_logs")
        };
        openLogsButton.Click += (_, _) => OpenLogsFolder();

        statusTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000
        };
        statusTimer.Tick += (_, _) => RefreshRuntimeStatus();

        Controls.Add(titleLabel);
        Controls.Add(hintLabel);
        Controls.Add(presetCaptionLabel);
        Controls.Add(presetValueLabel);
        Controls.Add(summaryCaptionLabel);
        Controls.Add(summaryValueLabel);
        Controls.Add(runtimeStatusCaptionLabel);
        Controls.Add(runtimeStatusValueLabel);
        Controls.Add(connectionHealthCaptionLabel);
        Controls.Add(connectionHealthLampLabel);
        Controls.Add(connectionHealthValueLabel);
        Controls.Add(statusLabel);
        Controls.Add(testButton);
        Controls.Add(startButton);
        Controls.Add(stopButton);
        Controls.Add(settingsButton);
        Controls.Add(exportSupportButton);
        Controls.Add(openLogsButton);

        Load += (_, _) =>
        {
            LoadInitialState();
            statusTimer.Start();
            RefreshRuntimeStatus();
        };
        FormClosed += (_, _) => statusTimer.Stop();
    }

    private void LoadInitialState()
    {
        try
        {
            UiLog.Write("Loading operator UI state.");
            settingsData = SettingsStorage.Load(workspacePaths, ConfigPath);
            SettingsStorage.EnsureSelectedPreset(settingsData);

            if (SettingsStorage.GetSelectedPreset(settingsData) is PresetData preset)
            {
                if (!RuntimeConfigService.TryApplyPreset(BaseDir, preset, out _, out string error))
                {
                    UiLog.Write($"Initial preset apply failed: {error}");
                    statusLabel.Text = UiText.Text("status.initial_apply_failed");
                }
                else
                {
                    statusLabel.Text = UiText.Format("status.loaded_preset", preset.Name);
                }
            }
            else
            {
                statusLabel.Text = UiText.Text("status.no_presets");
            }

            UpdateSelectedPresetDisplay();
        }
        catch (Exception ex)
        {
            UiLog.WriteException("Failed to load initial state", ex);
            settingsData = new SettingsData();
            statusLabel.Text = UiText.Text("status.failed_to_load");
            UpdateSelectedPresetDisplay();
            MessageBox.Show(this, ex.Message, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateSelectedPresetDisplay()
    {
        PresetData? preset = SettingsStorage.GetSelectedPreset(settingsData);
        if (preset is null)
        {
            presetValueLabel.Text = UiText.Text("main.no_preset");
            summaryValueLabel.Text = UiText.Text("main.no_summary");
            startButton.Enabled = false;
            testButton.Enabled = false;
            return;
        }

        presetValueLabel.Text = preset.Name;
        summaryValueLabel.Text = BuildMaskedConnectionSummary(preset.ConnectionString.Trim(), preset.ConnectionType);
        startButton.Enabled = true;
        testButton.Enabled = true;
    }

    private void StartSelectedPreset()
    {
        if (SettingsStorage.GetSelectedPreset(settingsData) is not PresetData preset)
        {
            MessageBox.Show(this, UiText.Text("message.no_preset_to_start"), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!RuntimeConfigService.TryApplyPreset(BaseDir, preset, out _, out string error))
        {
            UiLog.Write($"Start rejected for preset \"{preset.Name}\": {error}");
            statusLabel.Text = error;
            MessageBox.Show(this, error, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LaunchHelper(StartExeName);
    }

    private void TestSelectedPreset()
    {
        if (SettingsStorage.GetSelectedPreset(settingsData) is not PresetData preset)
        {
            MessageBox.Show(this, UiText.Text("message.no_preset_to_test"), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        UiLog.Write($"Testing selected preset: {preset.Name} ({preset.ConnectionType}).");
        ConfigCheckResult result = RuntimeConfigService.RunCheck(BaseDir, preset.ConnectionString, preset.ConnectionType);
        statusLabel.Text = result.Success ? UiText.Text("status.config_test_passed") : UiText.Text("status.config_test_failed");
        MessageBox.Show(
            this,
            result.Message,
            UiText.Text("app.title"),
            MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void OpenSettings()
    {
        if (!EnsureSettingsAccess(interactive: true))
        {
            statusLabel.Text = UiText.Text("status.settings_locked");
            return;
        }

        using var dialog = new SettingsDialog(BaseDir, workspacePaths, settingsData);
        dialog.ShowDialog(this);

        settingsData = SettingsStorage.Load(workspacePaths, ConfigPath);
        SettingsStorage.EnsureSelectedPreset(settingsData);
        UpdateSelectedPresetDisplay();

        if (dialog.ChangesPersisted)
        {
            UiLog.Write("Settings dialog persisted changes.");
            statusLabel.Text = UiText.Text("status.settings_applied");
        }
        else
        {
            UiLog.Write("Settings dialog closed without persisted changes.");
            statusLabel.Text = UiText.Text("status.settings_closed");
        }
    }

    private bool EnsureSettingsAccess(bool interactive)
    {
        if (File.Exists(Path.Combine(BaseDir, ProModeFlagFileName)))
        {
            return true;
        }

        if (proModeUnlockedForSession)
        {
            return true;
        }

        string? configuredPassword = Environment.GetEnvironmentVariable(ProModePasswordEnvVar);
        if (string.IsNullOrWhiteSpace(configuredPassword))
        {
            if (interactive)
            {
                MessageBox.Show(
                    this,
                    UiText.Format("message.settings_locked_info", ProModeFlagFileName, ProModePasswordEnvVar),
                    UiText.Text("app.title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            return false;
        }

        if (!interactive)
        {
            return false;
        }

        string? enteredPassword = PromptDialogs.ShowTextPrompt(
            this,
            UiText.Text("dialog.unlock_settings_title"),
            UiText.Text("dialog.unlock_settings_prompt"),
            string.Empty,
            usePasswordMask: true);

        if (enteredPassword is null)
        {
            UiLog.Write("Settings unlock canceled.");
            return false;
        }

        if (!string.Equals(enteredPassword, configuredPassword, StringComparison.Ordinal))
        {
            UiLog.Write("Settings unlock failed: invalid password.");
            MessageBox.Show(this, UiText.Text("message.invalid_settings_password"), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        proModeUnlockedForSession = true;
        UiLog.Write("Settings unlocked for current session.");
        return true;
    }

    private void LaunchHelper(string fileName)
    {
        string helperPath = Path.Combine(BaseDir, fileName);
        if (!File.Exists(helperPath))
        {
            UiLog.Write($"Launch rejected: missing helper {helperPath}");
            MessageBox.Show(this, UiText.Format("message.missing_file", helperPath), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            UiLog.Write($"Launching helper {fileName}.");
            Process? process = Process.Start(new ProcessStartInfo
            {
                FileName = helperPath,
                WorkingDirectory = BaseDir,
                UseShellExecute = true
            });

            statusLabel.Text = UiText.Format("status.helper_launched", fileName);
            UiLog.Write(process is null
                ? $"{fileName} launch returned null process."
                : $"{fileName} launch requested. PID={process.Id}.");
        }
        catch (Exception ex)
        {
            UiLog.WriteException($"Failed to launch {fileName}", ex);
            statusLabel.Text = UiText.Format("status.helper_launch_failed", fileName);
            MessageBox.Show(this, ex.Message, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportSupportBundle()
    {
        string supportDir = Path.Combine(BaseDir, "support");
        string defaultFileName = $"AgentBrowser_Support_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

        using var dialog = new SaveFileDialog
        {
            Title = UiText.Text("dialog.export_bundle_title"),
            InitialDirectory = supportDir,
            FileName = defaultFileName,
            Filter = "Zip archive (*.zip)|*.zip",
            DefaultExt = "zip",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            UiLog.Write("Support bundle export canceled by user.");
            statusLabel.Text = UiText.Text("status.bundle_export_canceled");
            return;
        }

        try
        {
            UiLog.Write($"Creating support bundle: {dialog.FileName}");
            SupportBundleResult result = SupportBundleService.CreateBundle(BaseDir, dialog.FileName);
            if (!result.Success)
            {
                string message = result.ErrorMessage ?? UiText.Text("status.bundle_export_failed");
                UiLog.Write($"Support bundle export failed: {message}");
                statusLabel.Text = UiText.Text("status.bundle_export_failed");
                MessageBox.Show(this, message, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UiLog.Write($"Support bundle exported: {result.BundlePath} ({result.IncludedFileCount} files)");
            statusLabel.Text = UiText.Text("status.bundle_exported");
            MessageBox.Show(
                this,
                UiText.Format("message.bundle_exported", result.BundlePath ?? string.Empty, result.IncludedFileCount, Environment.NewLine),
                UiText.Text("app.title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            UiLog.WriteException("Support bundle export failed with exception", ex);
            statusLabel.Text = UiText.Text("status.bundle_export_failed");
            MessageBox.Show(this, ex.Message, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenLogsFolder()
    {
        try
        {
            UiLog.Write($"Opening logs folder: {BaseDir}");
            Process.Start(new ProcessStartInfo
            {
                FileName = BaseDir,
                UseShellExecute = true
            });

            statusLabel.Text = UiText.Text("status.logs_opened");
        }
        catch (Exception ex)
        {
            UiLog.WriteException("Open logs folder failed", ex);
            statusLabel.Text = UiText.Text("status.logs_open_failed");
            MessageBox.Show(this, ex.Message, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshRuntimeStatus()
    {
        SessionStatus sessionStatus = SessionLifecycleService.LoadStatus(workspacePaths);
        WorkspaceRuntimeLayout runtimeLayout = WorkspaceRuntimeService.Discover(BaseDir);
        WorkspaceRuntimeSnapshot runtimeSnapshot = WorkspaceRuntimeService.Inspect(runtimeLayout);
        SessionState effectiveState = ResolveEffectiveSessionState(
            sessionStatus.State,
            runtimeSnapshot.SingBoxProcessCount,
            runtimeSnapshot.BrowserProcessCount);

        runtimeStatusValueLabel.Text = UiText.FormatSessionState(effectiveState);
        runtimeStatusValueLabel.ForeColor = GetSessionStateColor(effectiveState);
        stopButton.Enabled = runtimeSnapshot.IsBrowserRunning || runtimeSnapshot.IsSingBoxRunning || effectiveState is SessionState.Starting or SessionState.Running or SessionState.Stopping;
        UpdateConnectionHealth(effectiveState);
    }

    private static SessionState ResolveEffectiveSessionState(SessionState storedState, int singBoxCount, int browserCount)
    {
        bool hasLiveProcesses = singBoxCount > 0 || browserCount > 0;

        if (hasLiveProcesses)
        {
            return storedState is SessionState.Starting or SessionState.Stopping
                ? storedState
                : SessionState.Running;
        }

        return storedState is SessionState.Starting or SessionState.Running or SessionState.Stopping
            ? SessionState.Stopped
            : storedState;
    }

    private static Color GetSessionStateColor(SessionState state)
    {
        return state switch
        {
            SessionState.Starting => Color.DarkOrange,
            SessionState.Running => Color.DarkGreen,
            SessionState.Stopping => Color.DarkOrange,
            SessionState.Error => Color.DarkRed,
            _ => Color.DarkRed
        };
    }

    private void UpdateConnectionHealth(SessionState effectiveState)
    {
        if (effectiveState != lastEffectiveState)
        {
            lastEffectiveState = effectiveState;

            if (effectiveState is not SessionState.Running)
            {
                connectivityState = effectiveState switch
                {
                    SessionState.Starting => ConnectivityState.Checking,
                    SessionState.Stopping => ConnectivityState.Checking,
                    SessionState.Error => ConnectivityState.Offline,
                    _ => ConnectivityState.Idle
                };
                connectivityStatusText = effectiveState switch
                {
                    SessionState.Starting => UiText.Text("connection.tunnel_starting"),
                    SessionState.Stopping => UiText.Text("connection.tunnel_stopping"),
                    SessionState.Error => UiText.Text("connection.session_error"),
                    _ => UiText.Text("connection.tunnel_stopped")
                };
                RenderConnectivityState();
            }
            else
            {
                lastConnectivityCheckUtc = DateTime.MinValue;
            }
        }

        if (effectiveState is not SessionState.Running)
        {
            return;
        }

        if (connectivityCheckInProgress)
        {
            return;
        }

        if (DateTime.UtcNow - lastConnectivityCheckUtc < ConnectivityRefreshInterval &&
            connectivityState is ConnectivityState.Online or ConnectivityState.Offline)
        {
            return;
        }

        connectivityCheckInProgress = true;
        connectivityState = ConnectivityState.Checking;
        connectivityStatusText = UiText.Text("connection.checking");
        RenderConnectivityState();
        UiLog.Write("Starting connectivity probe.");
        _ = ProbeConnectionAsync();
    }

    private async Task ProbeConnectionAsync()
    {
        string statusText;
        ConnectivityState status;

        try
        {
            using HttpResponseMessage response = await ConnectivityHttpClient.GetAsync("https://api.ipify.org/", HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            string ip = (await response.Content.ReadAsStringAsync()).Trim();

            status = ConnectivityState.Online;
            statusText = string.IsNullOrWhiteSpace(ip)
                ? UiText.Text("connection.connected")
                : UiText.Format("connection.connected_via", ip);
            UiLog.Write($"Connectivity probe succeeded: {statusText}");
        }
        catch (Exception ex)
        {
            status = ConnectivityState.Offline;
            statusText = UiText.Format("connection.failed", ex.GetType().Name);
            UiLog.Write($"Connectivity probe failed: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                connectivityCheckInProgress = false;
                lastConnectivityCheckUtc = DateTime.UtcNow;
                connectivityState = status;
                connectivityStatusText = statusText;
                RenderConnectivityState();
            }));
        }
        catch
        {
            // Ignore shutdown race conditions.
        }
    }

    private void RenderConnectivityState()
    {
        connectionHealthLampLabel.ForeColor = connectivityState switch
        {
            ConnectivityState.Online => Color.DarkGreen,
            ConnectivityState.Checking => Color.DarkOrange,
            ConnectivityState.Offline => Color.DarkRed,
            _ => Color.Gray
        };
        connectionHealthValueLabel.Text = connectivityStatusText;
    }

    private static HttpClient CreateConnectivityHttpClient()
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    private static string BuildMaskedConnectionSummary(string input, ConnectionKind kind)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return UiText.Text("summary.hidden");
        }

        if (kind == ConnectionKind.Vless && ConnectionParser.TryParseVless(input, out VlessConnection vless, out _))
        {
            string security = string.IsNullOrWhiteSpace(vless.Security) ? "tls" : vless.Security;
            return UiText.Format("summary.vless", MaskHost(vless.Host), vless.Port, security, MaskToken(vless.Uuid));
        }

        if (ConnectionParser.TryParseSocks(input, out SocksConnection socks, out _))
        {
            string authMode = string.IsNullOrWhiteSpace(socks.Username)
                ? UiText.Text("summary.socks_no_auth")
                : UiText.Text("summary.socks_auth_set");
            return UiText.Format("summary.socks", MaskHost(socks.Host), socks.Port, authMode);
        }

        return UiText.Text("summary.hidden");
    }

    private static string MaskHost(string host)
    {
        if (IPAddress.TryParse(host, out IPAddress? address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            string[] parts = host.Split('.');
            return parts.Length == 4 ? $"{parts[0]}.{parts[1]}.*.*" : "***";
        }

        if (host.Length <= 4)
        {
            return "***";
        }

        int lastDot = host.LastIndexOf('.');
        if (lastDot > 1)
        {
            return $"{host[0]}***{host[lastDot..]}";
        }

        return $"{host[0]}***{host[^1]}";
    }

    private static string MaskToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "****";
        }

        string compact = value.Trim();
        return compact.Length <= 8
            ? "****"
            : $"{compact[..4]}...{compact[^4..]}";
    }
}

internal sealed class SettingsDialog : Form
{
    private readonly WorkspacePaths workspacePaths;
    private readonly string baseDir;
    private readonly string configPath;
    private readonly ComboBox presetComboBox;
    private readonly TextBox presetNameTextBox;
    private readonly ComboBox connectionTypeComboBox;
    private readonly TextBox connectionTextBox;
    private readonly Label exampleLabel;
    private readonly Label validationLabel;
    private readonly Label statusLabel;
    private readonly Button duplicatePresetButton;
    private readonly Button renamePresetButton;
    private readonly Button deletePresetButton;
    private readonly Button applyButton;
    private readonly Button testButton;
    private readonly Button closeButton;

    private SettingsData settingsData;
    private string currentPresetName = string.Empty;
    private bool suppressPresetEvents;
    private bool suppressEditorEvents;
    private bool hasUnsavedChanges;

    public bool ChangesPersisted { get; private set; }

    public SettingsDialog(string baseDir, WorkspacePaths workspacePaths, SettingsData currentSettings)
    {
        this.baseDir = baseDir;
        this.workspacePaths = workspacePaths;
        configPath = Path.Combine(baseDir, "core", "config.json");
        settingsData = SettingsStorage.Clone(currentSettings);
        SettingsStorage.EnsureSelectedPreset(settingsData);

        Text = UiText.Text("settings.title");
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(920, 430);

        var hintLabel = new Label
        {
            Left = 16,
            Top = 16,
            Width = 888,
            Height = 38,
            Text = UiText.Text("settings.hint")
        };

        var presetCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 16,
            Top = 68,
            Text = UiText.Text("settings.preset")
        };

        presetComboBox = new ComboBox
        {
            Left = 16,
            Top = 88,
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        presetComboBox.SelectedIndexChanged += (_, _) => OnPresetSelectionChanged();

        duplicatePresetButton = new Button
        {
            Left = 176,
            Top = 86,
            Width = 78,
            Height = 28,
            Text = UiText.Text("button.duplicate")
        };
        duplicatePresetButton.Click += (_, _) => DuplicateSelectedPreset();

        renamePresetButton = new Button
        {
            Left = 264,
            Top = 86,
            Width = 78,
            Height = 28,
            Text = UiText.Text("button.rename")
        };
        renamePresetButton.Click += (_, _) => RenameSelectedPreset();

        deletePresetButton = new Button
        {
            Left = 352,
            Top = 86,
            Width = 78,
            Height = 28,
            Text = UiText.Text("button.delete")
        };
        deletePresetButton.Click += (_, _) => DeleteSelectedPreset();

        var presetNameCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 438,
            Top = 68,
            Text = UiText.Text("settings.name")
        };

        presetNameTextBox = new TextBox
        {
            Left = 438,
            Top = 88,
            Width = 170
        };
        presetNameTextBox.TextChanged += (_, _) => OnEditorChanged();

        var connectionTypeCaptionLabel = new Label
        {
            AutoSize = true,
            Left = 620,
            Top = 68,
            Text = UiText.Text("settings.type")
        };

        connectionTypeComboBox = new ComboBox
        {
            Left = 620,
            Top = 88,
            Width = 110,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        connectionTypeComboBox.Items.AddRange(new object[] { "SOCKS5", "VLESS" });
        connectionTypeComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (suppressEditorEvents)
            {
                return;
            }

            RefreshExampleText();
            RefreshValidationState();
            hasUnsavedChanges = true;
        };

        exampleLabel = new Label
        {
            Left = 16,
            Top = 126,
            Width = 888,
            Height = 40,
            Font = new Font("Consolas", 9F),
            Text = string.Empty
        };

        connectionTextBox = new TextBox
        {
            Left = 16,
            Top = 172,
            Width = 888,
            Height = 120,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10F),
            AcceptsReturn = true,
            AcceptsTab = false
        };
        connectionTextBox.TextChanged += (_, _) => OnEditorChanged();

        validationLabel = new Label
        {
            Left = 16,
            Top = 304,
            Width = 888,
            Height = 24,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        statusLabel = new Label
        {
            Left = 16,
            Top = 330,
            Width = 888,
            Height = 36,
            Text = UiText.Text("settings.ready")
        };

        applyButton = new Button
        {
            Left = 16,
            Top = 384,
            Width = 130,
            Height = 32,
            Text = UiText.Text("button.apply")
        };
        applyButton.Click += (_, _) => ApplyCurrentConfiguration(showSuccess: true);

        testButton = new Button
        {
            Left = 156,
            Top = 384,
            Width = 120,
            Height = 32,
            Text = UiText.Text("button.test")
        };
        testButton.Click += (_, _) => TestCurrentConfiguration();

        closeButton = new Button
        {
            Left = 286,
            Top = 384,
            Width = 120,
            Height = 32,
            Text = UiText.Text("button.close")
        };
        closeButton.Click += (_, _) => Close();

        Controls.Add(hintLabel);
        Controls.Add(presetCaptionLabel);
        Controls.Add(presetComboBox);
        Controls.Add(duplicatePresetButton);
        Controls.Add(renamePresetButton);
        Controls.Add(deletePresetButton);
        Controls.Add(presetNameCaptionLabel);
        Controls.Add(presetNameTextBox);
        Controls.Add(connectionTypeCaptionLabel);
        Controls.Add(connectionTypeComboBox);
        Controls.Add(exampleLabel);
        Controls.Add(connectionTextBox);
        Controls.Add(validationLabel);
        Controls.Add(statusLabel);
        Controls.Add(applyButton);
        Controls.Add(testButton);
        Controls.Add(closeButton);

        Load += (_, _) => LoadInitialState();
        FormClosing += OnFormClosing;
    }

    private void LoadInitialState()
    {
        PopulatePresetComboBox();

        PresetData? selectedPreset = SettingsStorage.GetSelectedPreset(settingsData);
        if (selectedPreset is null)
        {
            suppressEditorEvents = true;
            presetNameTextBox.Text = SettingsStorage.GetDefaultPresetName(ConnectionKind.Socks);
            connectionTypeComboBox.SelectedItem = "SOCKS5";
            connectionTextBox.Text = string.Empty;
            suppressEditorEvents = false;
            currentPresetName = string.Empty;
            statusLabel.Text = UiText.Text("settings.no_presets");
        }
        else
        {
            currentPresetName = selectedPreset.Name;
            presetComboBox.SelectedItem = selectedPreset.Name;
            LoadPresetIntoForm(selectedPreset);
            statusLabel.Text = UiText.Format("status.loaded_preset", selectedPreset.Name);
        }

        RefreshExampleText();
        RefreshValidationState();
        UpdatePresetManagementState();
        hasUnsavedChanges = false;
    }

    private void PopulatePresetComboBox()
    {
        suppressPresetEvents = true;
        presetComboBox.Items.Clear();

        foreach (PresetData preset in settingsData.Presets.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
        {
            presetComboBox.Items.Add(preset.Name);
        }

        suppressPresetEvents = false;
        UpdatePresetManagementState();
    }

    private void OnPresetSelectionChanged()
    {
        if (suppressPresetEvents)
        {
            return;
        }

        string? selectedName = presetComboBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selectedName) || string.Equals(selectedName, currentPresetName, StringComparison.Ordinal))
        {
            return;
        }

        if (!ConfirmDiscardChangesIfNeeded())
        {
            suppressPresetEvents = true;
            presetComboBox.SelectedItem = currentPresetName;
            suppressPresetEvents = false;
            return;
        }

        PresetData? preset = settingsData.Presets.FirstOrDefault(p => string.Equals(p.Name, selectedName, StringComparison.Ordinal));
        if (preset is null)
        {
            return;
        }

        currentPresetName = preset.Name;
        LoadPresetIntoForm(preset);
        RefreshValidationState();
        UpdatePresetManagementState();
        hasUnsavedChanges = false;
        statusLabel.Text = UiText.Format("status.loaded_preset", preset.Name);
        UiLog.Write($"Settings preset selected: {preset.Name} ({preset.ConnectionType}).");
    }

    private void LoadPresetIntoForm(PresetData preset)
    {
        suppressEditorEvents = true;
        presetNameTextBox.Text = preset.Name;
        connectionTypeComboBox.SelectedItem = SettingsStorage.FormatConnectionKind(preset.ConnectionType);
        connectionTextBox.Text = preset.ConnectionString;
        suppressEditorEvents = false;
        RefreshExampleText();
    }

    private void OnEditorChanged()
    {
        if (suppressEditorEvents)
        {
            return;
        }

        hasUnsavedChanges = true;
        RefreshExampleText();
        RefreshValidationState();
    }

    private void RefreshExampleText()
    {
        ConnectionKind kind = GetSelectedConnectionKind();
        exampleLabel.Text = kind switch
        {
            ConnectionKind.Vless => UiText.Text("settings.example_vless"),
            _ => UiText.Text("settings.example_socks")
        };
    }

    private void RefreshValidationState()
    {
        string input = connectionTextBox.Text.Trim();
        string presetName = presetNameTextBox.Text.Trim();
        ConnectionKind selectedKind = GetSelectedConnectionKind();

        if (string.IsNullOrWhiteSpace(input))
        {
            validationLabel.Text = UiText.Format("settings.validation_mode", SettingsStorage.FormatConnectionKind(selectedKind));
            applyButton.Enabled = false;
            testButton.Enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(presetName))
        {
            validationLabel.Text = UiText.Text("settings.validation_name_required");
            applyButton.Enabled = false;
            testButton.Enabled = false;
            return;
        }

        if (RuntimeConfigService.TryBuildConfig(input, selectedKind, out _, out string kindLabel, out string error))
        {
            validationLabel.Text = UiText.Format("settings.validation_ok", kindLabel);
            applyButton.Enabled = true;
            testButton.Enabled = true;
            statusLabel.Text = UiText.Format("settings.valid_config", kindLabel);
        }
        else
        {
            validationLabel.Text = UiText.Format("settings.validation_mode", SettingsStorage.FormatConnectionKind(selectedKind));
            applyButton.Enabled = false;
            testButton.Enabled = false;
            statusLabel.Text = error;
        }
    }

    private void UpdatePresetManagementState()
    {
        bool hasSelectedPreset = settingsData.Presets.Count > 0 && !string.IsNullOrWhiteSpace(currentPresetName);
        duplicatePresetButton.Enabled = hasSelectedPreset;
        renamePresetButton.Enabled = hasSelectedPreset;
        deletePresetButton.Enabled = settingsData.Presets.Count > 1 && hasSelectedPreset;
    }

    private void DuplicateSelectedPreset()
    {
        PresetData? source = settingsData.Presets.FirstOrDefault(p => string.Equals(p.Name, currentPresetName, StringComparison.Ordinal));
        if (source is null)
        {
            return;
        }

        string suggestedName = SettingsStorage.GetUniquePresetName(settingsData, $"{source.Name} Copy");
        string? newName = PromptDialogs.ShowTextPrompt(
            this,
            UiText.Text("dialog.duplicate_title"),
            UiText.Text("dialog.duplicate_prompt"),
            suggestedName,
            usePasswordMask: false);

        if (string.IsNullOrWhiteSpace(newName))
        {
            UiLog.Write($"Preset duplication canceled: {source.Name}");
            return;
        }

        newName = newName.Trim();
        if (settingsData.Presets.Any(p => string.Equals(p.Name, newName, StringComparison.Ordinal)))
        {
            MessageBox.Show(this, UiText.Format("message.preset_exists", newName), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var duplicatedPreset = new PresetData
        {
            Name = newName,
            ConnectionType = source.ConnectionType,
            ConnectionString = source.ConnectionString
        };

        settingsData.Presets.Add(duplicatedPreset);
        PopulatePresetComboBox();
        suppressPresetEvents = true;
        presetComboBox.SelectedItem = newName;
        suppressPresetEvents = false;
        currentPresetName = newName;
        LoadPresetIntoForm(duplicatedPreset);
        hasUnsavedChanges = true;
        statusLabel.Text = UiText.Format("status.preset_duplicated", source.Name);
        UiLog.Write($"Duplicated preset \"{source.Name}\" to \"{newName}\".");
    }

    private void RenameSelectedPreset()
    {
        PresetData? preset = settingsData.Presets.FirstOrDefault(p => string.Equals(p.Name, currentPresetName, StringComparison.Ordinal));
        if (preset is null)
        {
            return;
        }

        string? newName = PromptDialogs.ShowTextPrompt(
            this,
            UiText.Text("dialog.rename_title"),
            UiText.Text("dialog.rename_prompt"),
            preset.Name,
            usePasswordMask: false);

        if (string.IsNullOrWhiteSpace(newName))
        {
            UiLog.Write($"Preset rename canceled: {preset.Name}");
            return;
        }

        newName = newName.Trim();
        if (string.Equals(newName, preset.Name, StringComparison.Ordinal))
        {
            return;
        }

        if (settingsData.Presets.Any(p => !ReferenceEquals(p, preset) && string.Equals(p.Name, newName, StringComparison.Ordinal)))
        {
            MessageBox.Show(this, UiText.Format("message.preset_exists", newName), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string oldName = preset.Name;
        preset.Name = newName;
        currentPresetName = newName;
        suppressEditorEvents = true;
        presetNameTextBox.Text = newName;
        suppressEditorEvents = false;
        PopulatePresetComboBox();
        suppressPresetEvents = true;
        presetComboBox.SelectedItem = newName;
        suppressPresetEvents = false;
        hasUnsavedChanges = true;
        statusLabel.Text = UiText.Format("status.preset_renamed", oldName);
        UiLog.Write($"Renamed preset \"{oldName}\" to \"{newName}\".");
    }

    private void DeleteSelectedPreset()
    {
        if (settingsData.Presets.Count <= 1)
        {
            MessageBox.Show(this, UiText.Text("message.cannot_delete_last_preset"), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string? selectedName = currentPresetName;
        if (string.IsNullOrWhiteSpace(selectedName))
        {
            return;
        }

        DialogResult confirmation = MessageBox.Show(
            this,
            UiText.Format("message.delete_preset_confirm", selectedName),
            UiText.Text("app.title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
        {
            UiLog.Write($"Preset deletion canceled: {selectedName}");
            return;
        }

        int removed = settingsData.Presets.RemoveAll(p => string.Equals(p.Name, selectedName, StringComparison.Ordinal));
        if (removed == 0)
        {
            return;
        }

        PresetData nextPreset = settingsData.Presets[0];
        currentPresetName = nextPreset.Name;
        PopulatePresetComboBox();
        suppressPresetEvents = true;
        presetComboBox.SelectedItem = nextPreset.Name;
        suppressPresetEvents = false;
        LoadPresetIntoForm(nextPreset);
        hasUnsavedChanges = true;
        statusLabel.Text = UiText.Format("status.preset_deleted", selectedName);
        UiLog.Write($"Deleted preset \"{selectedName}\".");
    }

    private void ApplyCurrentConfiguration(bool showSuccess)
    {
        string presetName = presetNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(presetName))
        {
            UiLog.Write("Apply rejected: preset name is empty.");
            MessageBox.Show(this, UiText.Text("message.preset_name_required"), UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string input = connectionTextBox.Text.Trim();
        ConnectionKind selectedKind = GetSelectedConnectionKind();

        if (!RuntimeConfigService.TryBuildConfig(input, selectedKind, out JsonObject? config, out string kindLabel, out string error))
        {
            UiLog.Write($"Apply rejected for preset \"{presetName}\": {error}");
            statusLabel.Text = error;
            MessageBox.Show(this, error, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            File.WriteAllText(configPath, config.ToJsonString(SingBoxConfigBuilder.CreateIndentedJsonOptions()), new UTF8Encoding(false));

            SettingsStorage.UpsertPreset(settingsData, new PresetData
            {
                Name = presetName,
                ConnectionType = selectedKind,
                ConnectionString = input
            });
            settingsData.SelectedPresetName = presetName;
            SettingsStorage.Save(workspacePaths, settingsData);

            PopulatePresetComboBox();
            suppressPresetEvents = true;
            presetComboBox.SelectedItem = presetName;
            suppressPresetEvents = false;
            currentPresetName = presetName;
            hasUnsavedChanges = false;
            ChangesPersisted = true;

            statusLabel.Text = UiText.Format("status.settings_applied_preset", presetName, kindLabel);
            UiLog.Write($"Applied preset \"{presetName}\" as {kindLabel}.");

            if (showSuccess)
            {
                MessageBox.Show(
                    this,
                    UiText.Format("message.settings_applied_preset", presetName, kindLabel),
                    UiText.Text("app.title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            UiLog.WriteException($"Failed to apply preset \"{presetName}\"", ex);
            statusLabel.Text = UiText.Text("status.failed_to_write");
            MessageBox.Show(this, ex.Message, UiText.Text("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TestCurrentConfiguration()
    {
        string input = connectionTextBox.Text.Trim();
        ConnectionKind selectedKind = GetSelectedConnectionKind();
        UiLog.Write($"Testing configuration in settings for mode {selectedKind}.");

        ConfigCheckResult result = RuntimeConfigService.RunCheck(baseDir, input, selectedKind);
        statusLabel.Text = result.Success ? UiText.Text("status.config_test_passed") : UiText.Text("status.config_test_failed");
        MessageBox.Show(
            this,
            result.Message,
            UiText.Text("app.title"),
            MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscardChangesIfNeeded())
        {
            e.Cancel = true;
        }
    }

    private bool ConfirmDiscardChangesIfNeeded()
    {
        if (!hasUnsavedChanges)
        {
            return true;
        }

        DialogResult result = MessageBox.Show(
            this,
            UiText.Text("message.discard_changes"),
            UiText.Text("app.title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return false;
        }

        hasUnsavedChanges = false;
        return true;
    }

    private ConnectionKind GetSelectedConnectionKind()
    {
        return SettingsStorage.ParseConnectionKind(connectionTypeComboBox.SelectedItem?.ToString()) ?? ConnectionKind.Socks;
    }
}

internal static class SettingsStorage
{
    public static SettingsData Load(WorkspacePaths workspacePaths, string configPath)
    {
        SettingsData? workspaceSettings = TryLoadFromPath(workspacePaths.WorkspaceSettingsPath);
        if (workspaceSettings is not null)
        {
            return workspaceSettings;
        }

        SettingsData? legacySettings = TryLoadFromPath(workspacePaths.LegacySettingsPath);
        if (legacySettings is not null)
        {
            Save(workspacePaths, legacySettings);
            return legacySettings;
        }

        string? inferredConnection = InferConnectionFromConfig(configPath);
        if (!string.IsNullOrWhiteSpace(inferredConnection))
        {
            ConnectionKind kind = DetectConnectionKind(inferredConnection);
            var inferredSettings = new SettingsData
            {
                SelectedPresetName = "Imported",
                Presets =
                {
                    new PresetData
                    {
                        Name = "Imported",
                        ConnectionType = kind,
                        ConnectionString = inferredConnection
                    }
                }
            };

            Save(workspacePaths, inferredSettings);
            return inferredSettings;
        }

        return new SettingsData();
    }

    public static void Save(WorkspacePaths workspacePaths, SettingsData settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(workspacePaths.WorkspaceRootDir);
        File.WriteAllText(workspacePaths.WorkspaceSettingsPath, json, new UTF8Encoding(false));
        File.WriteAllText(workspacePaths.LegacySettingsPath, json, new UTF8Encoding(false));
    }

    public static SettingsData Clone(SettingsData source)
    {
        return new SettingsData
        {
            SelectedPresetName = source.SelectedPresetName,
            ProfessionalMode = source.ProfessionalMode,
            Presets = source.Presets
                .Select(p => new PresetData
                {
                    Name = p.Name,
                    ConnectionType = p.ConnectionType,
                    ConnectionString = p.ConnectionString
                })
                .ToList()
        };
    }

    public static void EnsureSelectedPreset(SettingsData settings)
    {
        Normalize(settings);

        if (settings.Presets.Count == 0)
        {
            settings.SelectedPresetName = string.Empty;
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.SelectedPresetName) ||
            settings.Presets.All(p => !string.Equals(p.Name, settings.SelectedPresetName, StringComparison.Ordinal)))
        {
            settings.SelectedPresetName = settings.Presets[0].Name;
        }
    }

    public static PresetData? GetSelectedPreset(SettingsData settings)
    {
        EnsureSelectedPreset(settings);
        return settings.Presets.FirstOrDefault(p => string.Equals(p.Name, settings.SelectedPresetName, StringComparison.Ordinal));
    }

    public static void UpsertPreset(SettingsData settings, PresetData preset)
    {
        PresetData? existing = settings.Presets.FirstOrDefault(p => string.Equals(p.Name, preset.Name, StringComparison.Ordinal));
        if (existing is null)
        {
            settings.Presets.Add(preset);
            return;
        }

        existing.ConnectionType = preset.ConnectionType;
        existing.ConnectionString = preset.ConnectionString;
    }

    public static string GetUniquePresetName(SettingsData settings, string baseName)
    {
        string candidate = baseName.Trim();
        if (settings.Presets.All(p => !string.Equals(p.Name, candidate, StringComparison.Ordinal)))
        {
            return candidate;
        }

        int suffix = 2;
        while (true)
        {
            string numberedCandidate = $"{candidate} {suffix}";
            if (settings.Presets.All(p => !string.Equals(p.Name, numberedCandidate, StringComparison.Ordinal)))
            {
                return numberedCandidate;
            }

            suffix++;
        }
    }

    public static string GetDefaultPresetName(ConnectionKind kind)
    {
        return kind switch
        {
            ConnectionKind.Vless => "Default VLESS",
            _ => "Default SOCKS"
        };
    }

    public static string FormatConnectionKind(ConnectionKind kind)
    {
        return kind switch
        {
            ConnectionKind.Vless => "VLESS",
            _ => "SOCKS5"
        };
    }

    public static ConnectionKind? ParseConnectionKind(string? value)
    {
        return ConnectionParser.TryParseConnectionKind(value);
    }

    private static void Normalize(SettingsData settings)
    {
        settings.Presets ??= new List<PresetData>();
        settings.Presets = settings.Presets
            .Where(p => !string.IsNullOrWhiteSpace(p.Name) && !string.IsNullOrWhiteSpace(p.ConnectionString))
            .ToList();
    }

    private static SettingsData? TryLoadFromPath(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        JsonNode? root = JsonNode.Parse(File.ReadAllText(path));
        if (root?["presets"] is JsonArray)
        {
            SettingsData settings = JsonSerializer.Deserialize<SettingsData>(root.ToJsonString()) ?? new SettingsData();
            Normalize(settings);
            return settings;
        }

        string? oldConnectionString = root?["connectionString"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(oldConnectionString))
        {
            ConnectionKind kind = ParseConnectionKind(root?["connectionType"]?.GetValue<string>()) ?? DetectConnectionKind(oldConnectionString);
            return new SettingsData
            {
                SelectedPresetName = GetDefaultPresetName(kind),
                Presets =
                {
                    new PresetData
                    {
                        Name = GetDefaultPresetName(kind),
                        ConnectionType = kind,
                        ConnectionString = oldConnectionString
                    }
                }
            };
        }

        return null;
    }

    private static string? InferConnectionFromConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            return null;
        }

        JsonNode? root = JsonNode.Parse(File.ReadAllText(configPath));
        JsonArray? outbounds = root?["outbounds"]?.AsArray();
        if (outbounds is null)
        {
            return null;
        }

        foreach (JsonNode? node in outbounds)
        {
            if (node is not JsonObject outbound)
            {
                continue;
            }

            string? type = outbound["type"]?.GetValue<string>();
            if (string.Equals(type, "socks", StringComparison.OrdinalIgnoreCase))
            {
                string server = outbound["server"]?.GetValue<string>() ?? string.Empty;
                int port = outbound["server_port"]?.GetValue<int>() ?? 0;
                string username = outbound["username"]?.GetValue<string>() ?? string.Empty;
                string password = outbound["password"]?.GetValue<string>() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(server) && port > 0)
                {
                    return string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password)
                        ? $"{server}:{port}"
                        : $"{server}:{port}:{username}:{password}";
                }
            }

            if (string.Equals(type, "vless", StringComparison.OrdinalIgnoreCase))
            {
                string server = outbound["server"]?.GetValue<string>() ?? string.Empty;
                int port = outbound["server_port"]?.GetValue<int>() ?? 0;
                string uuid = outbound["uuid"]?.GetValue<string>() ?? string.Empty;
                string flow = outbound["flow"]?.GetValue<string>() ?? string.Empty;

                JsonObject? tls = outbound["tls"] as JsonObject;
                JsonObject? reality = tls?["reality"] as JsonObject;
                JsonObject? utls = tls?["utls"] as JsonObject;

                string sni = tls?["server_name"]?.GetValue<string>() ?? server;
                string fp = utls?["fingerprint"]?.GetValue<string>() ?? "chrome";
                string pbk = reality?["public_key"]?.GetValue<string>() ?? string.Empty;
                string sid = reality?["short_id"]?.GetValue<string>() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(server) && port > 0 && !string.IsNullOrWhiteSpace(uuid))
                {
                    var builder = new StringBuilder();
                    builder.Append("vless://");
                    builder.Append(uuid);
                    builder.Append('@');
                    builder.Append(server);
                    builder.Append(':');
                    builder.Append(port);
                    builder.Append("?encryption=none");

                    if (!string.IsNullOrWhiteSpace(flow))
                    {
                        builder.Append("&flow=").Append(Uri.EscapeDataString(flow));
                    }

                    builder.Append("&type=tcp");
                    builder.Append(!string.IsNullOrWhiteSpace(pbk) ? "&security=reality" : "&security=tls");

                    if (!string.IsNullOrWhiteSpace(sni))
                    {
                        builder.Append("&sni=").Append(Uri.EscapeDataString(sni));
                    }

                    if (!string.IsNullOrWhiteSpace(fp))
                    {
                        builder.Append("&fp=").Append(Uri.EscapeDataString(fp));
                    }

                    if (!string.IsNullOrWhiteSpace(pbk))
                    {
                        builder.Append("&pbk=").Append(Uri.EscapeDataString(pbk));
                    }

                    if (!string.IsNullOrWhiteSpace(sid))
                    {
                        builder.Append("&sid=").Append(Uri.EscapeDataString(sid));
                    }

                    return builder.ToString();
                }
            }
        }

        return null;
    }

    private static ConnectionKind DetectConnectionKind(string input)
    {
        return ConnectionParser.DetectConnectionKind(input);
    }
}

internal static class RuntimeConfigService
{
    public static bool TryBuildConfig(string input, ConnectionKind kind, out JsonObject config, out string kindLabel, out string error)
    {
        return SingBoxConfigBuilder.TryBuildConfig(input, kind, out config, out kindLabel, out error);
    }

    public static bool TryApplyPreset(string baseDir, PresetData preset, out string kindLabel, out string error)
    {
        if (!TryBuildConfig(preset.ConnectionString, preset.ConnectionType, out JsonObject? config, out kindLabel, out error))
        {
            return false;
        }

        string configPath = Path.Combine(baseDir, "core", "config.json");
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        File.WriteAllText(configPath, config.ToJsonString(SingBoxConfigBuilder.CreateIndentedJsonOptions()), new UTF8Encoding(false));
        return true;
    }

    public static ConfigCheckResult RunCheck(string baseDir, string input, ConnectionKind kind)
    {
        if (!TryBuildConfig(input, kind, out JsonObject? config, out _, out string error))
        {
            UiLog.Write($"Config test rejected: {error}");
            return new ConfigCheckResult(false, error);
        }

        string singBoxExe = Path.Combine(baseDir, "core", "sing-box.exe");
        if (!File.Exists(singBoxExe))
        {
            UiLog.Write($"Config test failed: missing file {singBoxExe}");
            return new ConfigCheckResult(false, UiText.Format("message.missing_file", singBoxExe));
        }

        string tempDir = Path.Combine(Path.GetTempPath(), "AgentBrowserUiTest_" + Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(tempDir);
            string tempConfigPath = Path.Combine(tempDir, "config.json");
            File.WriteAllText(tempConfigPath, config.ToJsonString(SingBoxConfigBuilder.CreateIndentedJsonOptions()), new UTF8Encoding(false));

            var startInfo = new ProcessStartInfo
            {
                FileName = singBoxExe,
                WorkingDirectory = tempDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("-D");
            startInfo.ArgumentList.Add(tempDir);
            startInfo.ArgumentList.Add("-C");
            startInfo.ArgumentList.Add(tempDir);
            startInfo.ArgumentList.Add("check");

            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                throw new InvalidOperationException("Process.Start returned null for sing-box check.");
            }

            process.WaitForExit(15000);
            string stdout = process.StandardOutput.ReadToEnd().Trim();
            string stderr = process.StandardError.ReadToEnd().Trim();

            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
                throw new TimeoutException("sing-box check timed out.");
            }

            if (process.ExitCode == 0)
            {
                UiLog.Write("Config test passed.");
                return new ConfigCheckResult(true, UiText.Text("message.config_test_passed"));
            }

            string details = BuildProcessDetails(process.ExitCode, stdout, stderr);
            UiLog.Write($"Config test failed. {details.Replace(Environment.NewLine, " | ")}");
            return new ConfigCheckResult(false, details);
        }
        catch (Exception ex)
        {
            UiLog.WriteException("Config test failed with exception", ex);
            return new ConfigCheckResult(false, ex.Message);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
            catch
            {
                // Ignore temporary directory cleanup failures.
            }
        }
    }

    private static string BuildProcessDetails(int exitCode, string stdout, string stderr)
    {
        var parts = new List<string> { UiText.Format("message.config_test_failed_exit", exitCode) };
        if (!string.IsNullOrWhiteSpace(stdout))
        {
            parts.Add("stdout:");
            parts.Add(stdout);
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            parts.Add("stderr:");
            parts.Add(stderr);
        }

        return string.Join(Environment.NewLine, parts);
    }
}

internal sealed record ConfigCheckResult(bool Success, string Message);

internal static class PromptDialogs
{
    public static string? ShowTextPrompt(IWin32Window owner, string title, string prompt, string initialValue, bool usePasswordMask)
    {
        using var promptForm = new Form
        {
            Width = 420,
            Height = 170,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            Text = title
        };

        var promptLabel = new Label
        {
            Left = 12,
            Top = 12,
            Width = 380,
            Height = 34,
            Text = prompt
        };

        var promptTextBox = new TextBox
        {
            Left = 12,
            Top = 52,
            Width = 380,
            Text = initialValue
        };

        if (usePasswordMask)
        {
            promptTextBox.UseSystemPasswordChar = true;
        }

        var okButton = new Button
        {
            Left = 236,
            Top = 90,
            Width = 75,
            Text = UiText.Text("button.ok"),
            DialogResult = DialogResult.OK
        };

        var cancelButton = new Button
        {
            Left = 317,
            Top = 90,
            Width = 75,
            Text = UiText.Text("button.cancel"),
            DialogResult = DialogResult.Cancel
        };

        promptForm.Controls.Add(promptLabel);
        promptForm.Controls.Add(promptTextBox);
        promptForm.Controls.Add(okButton);
        promptForm.Controls.Add(cancelButton);
        promptForm.AcceptButton = okButton;
        promptForm.CancelButton = cancelButton;

        DialogResult result = promptForm.ShowDialog(owner);
        return result == DialogResult.OK ? promptTextBox.Text : null;
    }
}

internal sealed class SettingsData
{
    public string SelectedPresetName { get; set; } = string.Empty;
    public bool ProfessionalMode { get; set; }
    public List<PresetData> Presets { get; set; } = new();
}

internal sealed class PresetData
{
    public string Name { get; set; } = string.Empty;
    public ConnectionKind ConnectionType { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}

internal enum ConnectivityState
{
    Idle,
    Checking,
    Online,
    Offline
}

internal static class UiText
{
    private static readonly bool IsRussian = string.Equals(
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
        "ru",
        StringComparison.OrdinalIgnoreCase);

    private static readonly Dictionary<string, (string En, string Ru)> Strings = new()
    {
        ["app.title"] = ("Agent Browser UI", "Agent Browser UI"),
        ["error.unexpected_ui"] = ("Unexpected UI error. See ui.log for details.", "Непредвиденная ошибка интерфейса. Подробности смотрите в ui.log."),
        ["error.fatal_startup"] = ("AgentBrowserUI.exe failed to start. See ui.log for details.", "AgentBrowserUI.exe не запустился. Подробности смотрите в ui.log."),
        ["main.title"] = ("Agent Browser Control", "Управление Agent Browser"),
        ["main.hint"] = ("Basic mode is for operators: start, stop, test, and watch the connection state. Open Settings only for admin changes.", "Basic режим предназначен для оператора: запуск, остановка, тест и контроль состояния соединения. Настройки открывайте только для административных изменений."),
        ["main.current_preset"] = ("Current preset", "Текущий пресет"),
        ["main.connection_summary"] = ("Connection summary", "Сводка подключения"),
        ["main.status"] = ("Status", "Статус"),
        ["main.connection"] = ("Connection", "Соединение"),
        ["main.ready"] = ("Ready.", "Готово."),
        ["main.no_preset"] = ("No preset configured.", "Пресет не настроен."),
        ["main.no_summary"] = ("No connection is configured yet. Open Settings to add one.", "Подключение ещё не настроено. Откройте Настройки, чтобы добавить его."),
        ["button.test"] = ("Test", "Тест"),
        ["button.start"] = ("Start", "Пуск"),
        ["button.stop"] = ("Stop", "Стоп"),
        ["button.settings"] = ("Settings", "Настройки"),
        ["button.export_bundle"] = ("Export Bundle", "Экспорт лога"),
        ["button.open_logs"] = ("Open Logs", "Открыть логи"),
        ["button.duplicate"] = ("Duplicate", "Дублировать"),
        ["button.rename"] = ("Rename", "Переименовать"),
        ["button.delete"] = ("Delete", "Удалить"),
        ["button.apply"] = ("Apply", "Применить"),
        ["button.close"] = ("Close", "Закрыть"),
        ["button.ok"] = ("OK", "OK"),
        ["button.cancel"] = ("Cancel", "Отмена"),
        ["status.loaded_preset"] = ("Loaded preset \"{0}\".", "Загружен пресет \"{0}\"."),
        ["status.no_presets"] = ("No saved presets found.", "Сохранённые пресеты не найдены."),
        ["status.failed_to_load"] = ("Failed to load presets.", "Не удалось загрузить пресеты."),
        ["status.initial_apply_failed"] = ("Preset loaded, but applying core\\config.json failed.", "Пресет загружен, но запись core\\config.json не удалась."),
        ["status.helper_launched"] = ("{0} launched.", "{0} запущен."),
        ["status.helper_launch_failed"] = ("Failed to launch {0}.", "Не удалось запустить {0}."),
        ["status.bundle_export_canceled"] = ("Support bundle export canceled.", "Экспорт support bundle отменён."),
        ["status.bundle_export_failed"] = ("Support bundle export failed.", "Экспорт support bundle не удался."),
        ["status.bundle_exported"] = ("Support bundle exported.", "Support bundle экспортирован."),
        ["status.logs_opened"] = ("Logs folder opened.", "Папка с логами открыта."),
        ["status.logs_open_failed"] = ("Failed to open logs folder.", "Не удалось открыть папку с логами."),
        ["status.config_test_passed"] = ("Config test passed.", "Тест конфигурации пройден."),
        ["status.config_test_failed"] = ("Config test failed.", "Тест конфигурации не пройден."),
        ["status.settings_locked"] = ("Settings remain locked.", "Настройки остаются заблокированными."),
        ["status.settings_applied"] = ("Settings were applied.", "Настройки применены."),
        ["status.settings_closed"] = ("Settings window closed.", "Окно настроек закрыто."),
        ["status.failed_to_write"] = ("Failed to write configuration.", "Не удалось записать конфигурацию."),
        ["status.settings_applied_preset"] = ("Applied preset \"{0}\" as {1}.", "Применён пресет \"{0}\" как {1}."),
        ["status.preset_duplicated"] = ("Duplicated preset \"{0}\".", "Пресет \"{0}\" продублирован."),
        ["status.preset_renamed"] = ("Renamed preset \"{0}\".", "Пресет \"{0}\" переименован."),
        ["status.preset_deleted"] = ("Deleted preset \"{0}\".", "Пресет \"{0}\" удалён."),
        ["settings.title"] = ("Agent Browser Settings", "Настройки Agent Browser"),
        ["settings.hint"] = ("This screen is for admins. Change presets here, then click Apply to save the settings and activate the selected preset.", "Этот экран предназначен для администратора. Меняйте здесь пресеты, затем нажмите Применить, чтобы сохранить настройки и активировать выбранный пресет."),
        ["settings.preset"] = ("Preset", "Пресет"),
        ["settings.name"] = ("Name", "Имя"),
        ["settings.type"] = ("Type", "Тип"),
        ["settings.ready"] = ("Ready. Press Apply to persist admin changes.", "Готово. Нажмите Применить, чтобы сохранить административные изменения."),
        ["settings.no_presets"] = ("No saved presets. Enter a new preset and click Apply.", "Сохранённых пресетов нет. Введите новый пресет и нажмите Применить."),
        ["settings.example_vless"] = ("Example VLESS: vless://uuid@host:443?encryption=none&flow=xtls-rprx-vision&type=tcp&security=reality&sni=host&fp=chrome&pbk=...&sid=...", "Пример VLESS: vless://uuid@host:443?encryption=none&flow=xtls-rprx-vision&type=tcp&security=reality&sni=host&fp=chrome&pbk=...&sid=..."),
        ["settings.example_socks"] = ("Example SOCKS5: 50.118.198.32:64011:Ek5R3AJg:prGqb63R", "Пример SOCKS5: 50.118.198.32:64011:Ek5R3AJg:prGqb63R"),
        ["settings.validation_mode"] = ("Mode: {0}", "Режим: {0}"),
        ["settings.validation_ok"] = ("Mode: {0}", "Режим: {0}"),
        ["settings.validation_name_required"] = ("Preset name is required.", "Нужно указать имя пресета."),
        ["settings.valid_config"] = ("Configuration looks valid for {0}.", "Конфигурация выглядит корректной для {0}."),
        ["dialog.export_bundle_title"] = ("Export Support Bundle", "Экспорт Support Bundle"),
        ["dialog.unlock_settings_title"] = ("Unlock Settings", "Разблокировать настройки"),
        ["dialog.unlock_settings_prompt"] = ("Enter the settings password:", "Введите пароль для настроек:"),
        ["dialog.duplicate_title"] = ("Duplicate Preset", "Дублировать пресет"),
        ["dialog.duplicate_prompt"] = ("Enter a name for the duplicated preset:", "Введите имя для дубликата пресета:"),
        ["dialog.rename_title"] = ("Rename Preset", "Переименовать пресет"),
        ["dialog.rename_prompt"] = ("Enter a new preset name:", "Введите новое имя пресета:"),
        ["message.missing_file"] = ("Missing file:\n{0}", "Отсутствует файл:\n{0}"),
        ["message.bundle_exported"] = ("Support bundle exported:{2}{0}{2}{2}Included files: {1}", "Support bundle экспортирован:{2}{0}{2}{2}Включено файлов: {1}"),
        ["message.config_test_passed"] = ("Config test passed.", "Тест конфигурации пройден."),
        ["message.config_test_failed_exit"] = ("sing-box check failed. ExitCode={0}.", "Проверка sing-box не пройдена. ExitCode={0}."),
        ["message.no_preset_to_start"] = ("No preset is configured. Open Settings first.", "Пресет не настроен. Сначала откройте Настройки."),
        ["message.no_preset_to_test"] = ("No preset is configured. Open Settings first.", "Пресет не настроен. Сначала откройте Настройки."),
        ["message.settings_locked_info"] = ("Settings are locked by the administrator.{0}{0}Use one of these options:{0}- create {1} next to AgentBrowserUI.exe{0}- set environment variable {2} and unlock with the password", "Настройки заблокированы администратором.{0}{0}Используйте один из вариантов:{0}- создайте файл {1} рядом с AgentBrowserUI.exe{0}- задайте переменную среды {2} и разблокируйте настройки паролем"),
        ["message.invalid_settings_password"] = ("Invalid settings password.", "Неверный пароль для настроек."),
        ["message.preset_exists"] = ("Preset \"{0}\" already exists.", "Пресет \"{0}\" уже существует."),
        ["message.delete_preset_confirm"] = ("Delete preset \"{0}\"?", "Удалить пресет \"{0}\"?"),
        ["message.cannot_delete_last_preset"] = ("The last preset cannot be deleted. Create or duplicate another preset first.", "Последний пресет удалять нельзя. Сначала создайте или продублируйте другой пресет."),
        ["message.preset_name_required"] = ("Preset name is required.", "Нужно указать имя пресета."),
        ["message.settings_applied_preset"] = ("Preset \"{0}\" was saved and activated as {1}.", "Пресет \"{0}\" сохранён и активирован как {1}."),
        ["message.discard_changes"] = ("Discard unsaved settings changes?", "Отменить несохранённые изменения настроек?"),
        ["connection.tunnel_starting"] = ("Tunnel starting...", "Туннель запускается..."),
        ["connection.tunnel_stopping"] = ("Tunnel stopping...", "Туннель останавливается..."),
        ["connection.tunnel_stopped"] = ("Tunnel stopped.", "Туннель остановлен."),
        ["connection.session_error"] = ("Session error.", "Ошибка сессии."),
        ["connection.checking"] = ("Checking connection...", "Проверка соединения..."),
        ["connection.connected"] = ("Connected.", "Соединение установлено."),
        ["connection.connected_via"] = ("Connected via {0}", "Подключено через {0}"),
        ["connection.failed"] = ("Connection failed: {0}", "Соединение не удалось: {0}"),
        ["summary.hidden"] = ("Connection details are hidden in Basic mode.", "Детали подключения скрыты в Basic режиме."),
        ["summary.vless"] = ("Saved VLESS: host={0}, port={1}, security={2}, id={3}", "Сохранённый VLESS: host={0}, port={1}, security={2}, id={3}"),
        ["summary.socks"] = ("Saved SOCKS5: host={0}, port={1}, {2}", "Сохранённый SOCKS5: host={0}, port={1}, {2}"),
        ["summary.socks_no_auth"] = ("no auth", "без авторизации"),
        ["summary.socks_auth_set"] = ("auth set", "авторизация задана")
    };

    public static string Text(string key)
    {
        (string En, string Ru) value = Strings[key];
        return IsRussian ? value.Ru : value.En;
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, Text(key), args);
    }

    public static string FormatSessionState(SessionState state)
    {
        return state switch
        {
            SessionState.Starting => IsRussian ? "Запуск" : "Starting",
            SessionState.Running => IsRussian ? "Работает" : "Running",
            SessionState.Stopping => IsRussian ? "Остановка" : "Stopping",
            SessionState.Error => IsRussian ? "Ошибка" : "Error",
            _ => IsRussian ? "Остановлено" : "Stopped"
        };
    }
}
