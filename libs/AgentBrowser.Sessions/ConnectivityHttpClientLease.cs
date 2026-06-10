using AgentBrowser.Config;

namespace AgentBrowser.Sessions;

public sealed class ConnectivityHttpClientLease : IDisposable
{
    private readonly bool disposeClient;

    private ConnectivityHttpClientLease(HttpClient client, bool disposeClient)
    {
        Client = client;
        this.disposeClient = disposeClient;
    }

    public HttpClient Client { get; }

    public static ConnectivityHttpClientLease ForRuntimeMode(
        RuntimeMode runtimeMode,
        HttpClient directClient,
        Func<HttpClient> browserProxyClientFactory)
    {
        return runtimeMode == RuntimeMode.BrowserProxy
            ? new ConnectivityHttpClientLease(browserProxyClientFactory(), disposeClient: true)
            : new ConnectivityHttpClientLease(directClient, disposeClient: false);
    }

    public void Dispose()
    {
        if (disposeClient)
        {
            Client.Dispose();
        }
    }
}
