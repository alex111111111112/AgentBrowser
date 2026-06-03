using AgentBrowser.Config;
using Xunit;

namespace AgentBrowser.Config.Tests;

public sealed class SingBoxConfigBuilderRuntimeModeTests
{
    [Fact]
    public void InferRuntimeModeFromConfigPath_WhenConfigIsMissing_ReturnsBrowserProxy()
    {
        string configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "config.json");

        RuntimeMode runtimeMode = SingBoxConfigBuilder.InferRuntimeModeFromConfigPath(configPath);

        Assert.Equal(RuntimeMode.BrowserProxy, runtimeMode);
    }

    [Fact]
    public void InferRuntimeModeFromConfigPath_WhenConfigHasSocksInbound_ReturnsBrowserProxy()
    {
        using TempConfig config = TempConfig.Write("""
            {
              "inbounds": [
                {
                  "type": "socks",
                  "listen": "127.0.0.1",
                  "listen_port": 1080
                }
              ]
            }
            """);

        RuntimeMode runtimeMode = SingBoxConfigBuilder.InferRuntimeModeFromConfigPath(config.Path);

        Assert.Equal(RuntimeMode.BrowserProxy, runtimeMode);
    }

    [Fact]
    public void InferRuntimeModeFromConfigPath_WhenConfigHasTunInbound_ReturnsSystemTun()
    {
        using TempConfig config = TempConfig.Write("""
            {
              "inbounds": [
                {
                  "type": "tun",
                  "tag": "tun-in"
                }
              ]
            }
            """);

        RuntimeMode runtimeMode = SingBoxConfigBuilder.InferRuntimeModeFromConfigPath(config.Path);

        Assert.Equal(RuntimeMode.SystemTun, runtimeMode);
    }

    [Fact]
    public void InferRuntimeModeFromConfigPath_WhenConfigIsMalformed_ReturnsBrowserProxy()
    {
        using TempConfig config = TempConfig.Write("""{ "inbounds": [ { "type": "tun" }""");

        RuntimeMode runtimeMode = SingBoxConfigBuilder.InferRuntimeModeFromConfigPath(config.Path);

        Assert.Equal(RuntimeMode.BrowserProxy, runtimeMode);
    }

    [Fact]
    public void InferRuntimeModeFromConfigPath_WhenInboundsIsNotArray_ReturnsBrowserProxy()
    {
        using TempConfig config = TempConfig.Write("""
            {
              "inbounds": {
                "type": "tun"
              }
            }
            """);

        RuntimeMode runtimeMode = SingBoxConfigBuilder.InferRuntimeModeFromConfigPath(config.Path);

        Assert.Equal(RuntimeMode.BrowserProxy, runtimeMode);
    }

    [Fact]
    public void InferRuntimeModeFromConfigPath_WhenInboundTypeIsNotString_ReturnsBrowserProxy()
    {
        using TempConfig config = TempConfig.Write("""
            {
              "inbounds": [
                {
                  "type": 42
                }
              ]
            }
            """);

        RuntimeMode runtimeMode = SingBoxConfigBuilder.InferRuntimeModeFromConfigPath(config.Path);

        Assert.Equal(RuntimeMode.BrowserProxy, runtimeMode);
    }

    private sealed class TempConfig : IDisposable
    {
        private TempConfig(string directory, string path)
        {
            Directory = directory;
            Path = path;
        }

        public string Directory { get; }

        public string Path { get; }

        public static TempConfig Write(string content)
        {
            string directory = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "agentbrowser-config-tests",
                Guid.NewGuid().ToString("N"));

            System.IO.Directory.CreateDirectory(directory);
            string path = System.IO.Path.Combine(directory, "config.json");
            File.WriteAllText(path, content);
            return new TempConfig(directory, path);
        }

        public void Dispose()
        {
            if (System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.Delete(Directory, recursive: true);
            }
        }
    }
}
