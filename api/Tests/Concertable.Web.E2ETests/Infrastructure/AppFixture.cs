using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Concertable.Identity.Application.DTOs;
using Concertable.Seeding;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Net.Http.Headers;
using Xunit.Abstractions;

namespace Concertable.Web.E2ETests.Infrastructure;

public class AppFixture : IAsyncLifetime
{
    private DistributedApplication app = null!;
    private StripeCliFixture stripeCli = null!;
    private SqlFixture sqlFixture = null!;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<AppFixture> logger;

    internal const string ApiBaseUrl = "http://localhost:7001";

    public HttpClient Client { get; private set; } = null!;
    public IPollingService Polling { get; private set; } = null!;
    public PaymentIntentService StripePaymentIntents { get; private set; } = null!;
    public SeedDataResponse SeedData { get; private set; } = null!;

    public AppFixture(IMessageSink messageSink)
    {
        loggerFactory = LoggerFactory.Create(b => b
            .AddProvider(new MessageSinkLoggerProvider(messageSink))
            .SetMinimumLevel(LogLevel.Debug));

        logger = loggerFactory.CreateLogger<AppFixture>();
        Polling = new PollingService(loggerFactory.CreateLogger<PollingService>());
    }

    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing E2E test fixture");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Concertable_AppHost>();

        stripeCli = builder.AddStripe(ApiBaseUrl);
        await stripeCli.InitializeAsync();

        builder.AddE2E(stripeCli.WebhookSecret);
        StripePaymentIntents = new PaymentIntentService(new StripeClient(stripeCli.ApiKey));

        app = await builder.BuildAsync();
        await app.StartAsync();

        Client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };

        await WaitForAppAsync();

        sqlFixture = new SqlFixture();
        await sqlFixture.InitializeAsync(app);

        logger.LogInformation("E2E test fixture ready");
    }

    public async Task ResetAsync()
    {
        await sqlFixture.ResetAsync();
        var response = await Client.PostAsync("/e2e/reseed");
        SeedData = (await response.Content.ReadAsync<SeedDataResponse>())!;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var response = await Client.PostAsJsonEnsureSuccessAsync("/api/Auth/login", new { Email = email, Password = password, RememberMe = false });
        var login = await response.Content.ReadAsync<LoginResponse>();

        var httpClient = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return httpClient;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await sqlFixture.DisposeAsync();
        await app.DisposeAsync();
        await stripeCli.DisposeAsync();
        loggerFactory.Dispose();
    }

    private async Task WaitForAppAsync()
    {
        logger.LogInformation("Waiting for app to become healthy at {Url}/health", ApiBaseUrl);

        await Polling.UntilAsync(async () =>
        {
            var response = await Client.GetAsync("/health");
            logger.LogDebug("Health check: {StatusCode}", response.StatusCode);
            return response.IsSuccessStatusCode;
        },
        timeout: TimeSpan.FromSeconds(60),
        interval: TimeSpan.FromSeconds(1));

        logger.LogInformation("App is healthy");
    }
}
