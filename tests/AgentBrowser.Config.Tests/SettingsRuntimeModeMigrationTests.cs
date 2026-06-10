using System.Text.Json.Nodes;
using AgentBrowser.Config;
using Xunit;

namespace AgentBrowser.Config.Tests;

public sealed class SettingsRuntimeModeMigrationTests
{
    [Fact]
    public void ApplyMissingPresetRuntimeModes_WhenPresetHasNoRuntimeMode_UsesFallbackRuntimeMode()
    {
        JsonNode root = JsonNode.Parse("""
            {
              "selectedPresetName": "Imported",
              "presets": [
                {
                  "name": "Imported",
                  "connectionType": "Socks",
                  "connectionString": "socks5://127.0.0.1:1080"
                }
              ]
            }
            """)!;

        bool changed = SettingsRuntimeModeMigration.ApplyMissingPresetRuntimeModes(root, RuntimeMode.SystemTun);

        Assert.True(changed);
        Assert.Equal(
            RuntimeMode.SystemTun.ToString(),
            root["presets"]![0]!["runtimeMode"]!.GetValue<string>());
    }

    [Fact]
    public void ApplyMissingPresetRuntimeModes_WhenPresetHasRuntimeMode_DoesNotOverrideIt()
    {
        JsonNode root = JsonNode.Parse("""
            {
              "selectedPresetName": "Imported",
              "presets": [
                {
                  "name": "Imported",
                  "connectionType": "Socks",
                  "runtimeMode": "BrowserProxy",
                  "connectionString": "socks5://127.0.0.1:1080"
                }
              ]
            }
            """)!;

        bool changed = SettingsRuntimeModeMigration.ApplyMissingPresetRuntimeModes(root, RuntimeMode.SystemTun);

        Assert.False(changed);
        Assert.Equal(
            RuntimeMode.BrowserProxy.ToString(),
            root["presets"]![0]!["runtimeMode"]!.GetValue<string>());
    }

    [Fact]
    public void HasPresetArray_WhenSettingsUsePascalCase_ReturnsTrue()
    {
        JsonNode root = JsonNode.Parse("""
            {
              "SelectedPresetName": "Imported",
              "Presets": [
                {
                  "Name": "Imported",
                  "ConnectionType": "Socks",
                  "ConnectionString": "socks5://127.0.0.1:1080"
                }
              ]
            }
            """)!;

        Assert.True(SettingsRuntimeModeMigration.HasPresetArray(root));
    }
}
