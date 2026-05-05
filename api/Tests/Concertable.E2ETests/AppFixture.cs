using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Concertable.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Stripe;
using System.Net.Http.Headers;
using Xunit.Abstractions;

namespace Concertable.E2ETests;

public class AppFixture : IAsyncLifetime
{
    private DistributedApplication app = null!;
    private StripeCliFixture stripeCli = null!;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<AppFixture> logger;
    private readonly IConfiguration configuration;
    private readonly TestTokenMinter tokenMinter;

    public const string TestPaymentMethodId = "pm_card_visa";

    public string ApiBaseUrl { get; }
    public string AuthBaseUrl { get; }
    public string SpaBaseUrl { get; }
    public HttpClient Client { get; private set; } = null!;
    public IPollingService Polling { get; private set; } = null!;
    public PaymentIntentService StripePaymentIntents { get; private set; } = null!;
    public SeedDataResponse SeedData { get; private set; } = null!;
    public SqlFixture Sql { get; private set; } = null!;

    public AppFixture() : this(BuildConsoleLoggerFactory()) { }

    public AppFixture(IMessageSink messageSink) : this(BuildMessageSinkLoggerFactory(messageSink)) { }

    private AppFixture(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<AppFixture>();
        Polling = new PollingService(loggerFactory.CreateLogger<PollingService>());

        configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.E2E.json"))
            .Build();

        ApiBaseUrl = configuration["Endpoints:Api"]
            ?? throw new InvalidOperationException("Endpoints:Api is not configured in appsettings.E2E.json.");
        AuthBaseUrl = configuration["Endpoints:Auth"]
            ?? throw new InvalidOperationException("Endpoints:Auth is not configured in appsettings.E2E.json.");
        SpaBaseUrl = configuration["Endpoints:Spa"]
            ?? throw new InvalidOperationException("Endpoints:Spa is not configured in appsettings.E2E.json.");

        tokenMinter = new TestTokenMinter(configuration);
    }

    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing E2E test fixture");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Concertable_AppHost>();

        stripeCli = builder.AddStripe(configuration, ApiBaseUrl);
        await stripeCli.InitializeAsync();

        builder.AddE2E(ApiBaseUrl, AuthBaseUrl, stripeCli.WebhookSecret);
        StripePaymentIntents = new PaymentIntentService(new StripeClient(stripeCli.ApiKey));

        app = await builder.BuildAsync();
        await app.StartAsync();

        Client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };

        await WaitForAppAsync();

        Sql = new SqlFixture();
        await Sql.InitializeAsync(app);

        logger.LogInformation("E2E test fixture ready");
    }

    public async Task ResetAsync()
    {
        await Sql.ResetAsync();
        var response = await Client.PostAsync("/e2e/reseed");
        SeedData = (await response.Content.ReadAsync<SeedDataResponse>())!;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var token = await tokenMinter.MintAsync(email, SeedData.TestPassword);
        var client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        tokenMinter.Dispose();
        await Sql.DisposeAsync();
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

    private static ILoggerFactory BuildConsoleLoggerFactory() =>
        LoggerFactory.Create(b => b
            .AddSimpleConsole(o => o.SingleLine = true)
            .SetMinimumLevel(LogLevel.Debug));

    private static ILoggerFactory BuildMessageSinkLoggerFactory(IMessageSink messageSink) =>
        LoggerFactory.Create(b => b
            .AddProvider(new MessageSinkLoggerProvider(messageSink))
            .SetMinimumLevel(LogLevel.Debug));
}
