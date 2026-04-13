using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Concertable.Tests.Common;
using System.Net.Http.Headers;

namespace Concertable.Web.E2ETests.Infrastructure;

public class AppFixture : IAsyncLifetime
{
    private DistributedApplication app = null!;
    private StripeCliFixture stripeCli = null!;
    private SqlFixture sqlFixture = null!;

    internal const string ApiBaseUrl = "http://localhost:7001";

    public HttpClient Client { get; private set; } = null!;
    public HttpClient ArtistManagerClient { get; private set; } = null!;
    public HttpClient VenueManagerClient { get; private set; } = null!;
    public HttpClient CustomerClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        stripeCli = new StripeCliFixture(ApiBaseUrl);
        await stripeCli.InitializeAsync();

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Concertable_AppHost>();

        builder.AddE2E(stripeCli.WebhookSecret);

        app = await builder.BuildAsync();
        await app.StartAsync();

        Client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };

        sqlFixture = new SqlFixture();
        await sqlFixture.InitializeAsync(app);

        await WaitForAppAsync();

        ArtistManagerClient = await CreateAuthenticatedClientAsync("artistmanager1@test.com", "Password11!");
        VenueManagerClient = await CreateAuthenticatedClientAsync("venuemanager1@test.com", "Password11!");
        CustomerClient = await CreateAuthenticatedClientAsync("customer1@test.com", "Password11!");
    }

    public async Task ResetAsync() => await sqlFixture.ResetAsync();

    public async Task DisposeAsync()
    {
        ArtistManagerClient?.Dispose();
        VenueManagerClient?.Dispose();
        CustomerClient?.Dispose();
        Client.Dispose();
        await sqlFixture.DisposeAsync();
        await app.DisposeAsync();
        await stripeCli.DisposeAsync();
    }

    private async Task WaitForAppAsync()
    {
        await WaitHelper.UntilAsync(async () =>
        {
            try
            {
                var response = await Client.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }, timeout: TimeSpan.FromSeconds(60), interval: TimeSpan.FromSeconds(1));
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var response = await Client.PostAsJsonEnsureSuccessAsync("/api/Auth/login", new { Email = email, Password = password, RememberMe = false });
        var login = await response.Content.ReadAsync<LoginResponse>();

        var httpClient = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return httpClient;
    }
}
