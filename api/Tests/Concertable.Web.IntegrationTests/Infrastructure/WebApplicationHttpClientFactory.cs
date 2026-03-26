using Microsoft.AspNetCore.Mvc.Testing;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class WebApplicationHttpClientFactory : IHttpClientFactory
{
    private readonly WebApplicationFactory<Program> factory;

    public WebApplicationHttpClientFactory(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    public HttpClient CreateClient(string name) => factory.CreateClient();
}
