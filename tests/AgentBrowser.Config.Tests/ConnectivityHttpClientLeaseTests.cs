using System.Net;
using AgentBrowser.Config;
using AgentBrowser.Sessions;
using Xunit;

namespace AgentBrowser.Config.Tests;

public sealed class ConnectivityHttpClientLeaseTests
{
    [Fact]
    public async Task ForRuntimeMode_WhenSystemTun_DoesNotDisposeSharedDirectClient()
    {
        using var directClient = new HttpClient(new StaticResponseHandler("198.51.100.10"));

        using (ConnectivityHttpClientLease lease = ConnectivityHttpClientLease.ForRuntimeMode(
            RuntimeMode.SystemTun,
            directClient,
            () => throw new InvalidOperationException("Browser proxy client should not be created.")))
        {
            Assert.Same(directClient, lease.Client);
        }

        string body = await directClient.GetStringAsync("https://example.test/");

        Assert.Equal("198.51.100.10", body);
    }

    [Fact]
    public async Task ForRuntimeMode_WhenBrowserProxy_DisposesTemporaryClient()
    {
        var temporaryClient = new HttpClient(new StaticResponseHandler("203.0.113.5"));
        using var directClient = new HttpClient(new StaticResponseHandler("198.51.100.10"));

        using (ConnectivityHttpClientLease lease = ConnectivityHttpClientLease.ForRuntimeMode(
            RuntimeMode.BrowserProxy,
            directClient,
            () => temporaryClient))
        {
            Assert.Same(temporaryClient, lease.Client);
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => temporaryClient.GetAsync("https://example.test/"));
    }

    private sealed class StaticResponseHandler : HttpMessageHandler
    {
        private readonly string body;

        public StaticResponseHandler(string body)
        {
            this.body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body)
            });
        }
    }
}
