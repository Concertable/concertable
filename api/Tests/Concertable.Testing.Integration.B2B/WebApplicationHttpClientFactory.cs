using Microsoft.AspNetCore.Mvc.Testing;

namespace Concertable.Testing.Integration.B2B;

public class WebApplicationHttpClientFactory : IHttpClientFactory
{
    private readonly WebApplicationFactory<Program> factory;

    public WebApplicationHttpClientFactory(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    public HttpClient CreateClient(string name) => factory.CreateClient();
}
