using Microsoft.AspNetCore.Mvc.Testing;

namespace Concertable.IntegrationTests.Common;

public class WebApplicationHttpClientFactory : IHttpClientFactory
{
    private readonly WebApplicationFactory<Program> factory;

    public WebApplicationHttpClientFactory(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    public HttpClient CreateClient(string name) => factory.CreateClient();
}
