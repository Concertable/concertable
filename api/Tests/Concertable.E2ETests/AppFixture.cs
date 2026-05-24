using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Concertable.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe;
using System.Net;
using System.Net.Http.Headers;
using Xunit.Abstractions;

namespace Concertable.E2ETests;

public class AppFixture : IAsyncLifetime
{
    private DistributedApplication app = null!;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<AppFixture> logger;
    private readonly IConfiguration configuration;
    private readonly TestTokenMinter tokenMinter;

    public const string TestPaymentMethodId = "pm_card_visa";

    public string ApiBaseUrl { get; }
    public string CustomerApiBaseUrl { get; }
    public string SearchApiBaseUrl { get; }
    public string AuthBaseUrl { get; }
    public string CustomerSpaUrl { get; }
    public string VenueSpaUrl { get; }
    public string ArtistSpaUrl { get; }
    public string BusinessSpaUrl { get; }
    public HttpClient Client { get; private set; } = null!;
    public IPollingService Polling { get; private set; } = null!;
    public PaymentIntentService StripePaymentIntents { get; private set; } = null!;
    public StripeFixture Stripe { get; private set; } = null!;
    public SeedDataResponse SeedData { get; private set; } = null!;
    public SqlFixture Sql { get; private set; } = null!;
    public TestDb Db { get; private set; } = null!;

    public AppFixture() : this(NullLoggerFactory.Instance) { }
    public AppFixture(IMessageSink messageSink) : this(BuildMessageSinkLoggerFactory(messageSink)) { }

    private AppFixture(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<AppFixture>();
        Polling = new PollingService(loggerFactory.CreateLogger<PollingService>());

        configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.E2E.json"))
            .AddEnvironmentVariables()
            .Build();

        var endpoints = configuration.GetSection("Endpoints").Get<E2EEndpoints>()
            ?? throw new InvalidOperationException("Endpoints section is missing from appsettings.E2E.json.");

        ApiBaseUrl         = endpoints.B2BWeb;
        CustomerApiBaseUrl = endpoints.CustomerWeb;
        SearchApiBaseUrl   = endpoints.SearchWeb;
        AuthBaseUrl        = endpoints.Auth;
        CustomerSpaUrl     = endpoints.CustomerSpa;
        VenueSpaUrl        = endpoints.VenueSpa;
        ArtistSpaUrl       = endpoints.ArtistSpa;
        BusinessSpaUrl     = endpoints.BusinessSpa;

        tokenMinter = new TestTokenMinter(configuration);
    }

    public async Task InitializeAsync()
    {
        logger.InitializingE2ETestFixture();

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Concertable_AppHost>();

        builder.AddE2E(ApiBaseUrl, CustomerApiBaseUrl, SearchApiBaseUrl, AuthBaseUrl);
        var stripeClient = new StripeClient(configuration["Stripe:SecretKey"]);
        StripePaymentIntents = new PaymentIntentService(stripeClient);
        Stripe = new StripeFixture(stripeClient);

        app = await builder.BuildAsync();
        await app.StartAsync();

        Client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };

        await WaitForAppAsync();

        Sql = new SqlFixture();
        await Sql.InitializeAsync(app);
        Db = new TestDb(Sql.Connection, Sql.PaymentConnection);

        logger.E2ETestFixtureReady();
    }

    public async Task ResetAsync()
    {
        Stripe.Reset();
        await Sql.ResetAsync();
        var response = await Client.PostAsync("/e2e/reseed");
        SeedData = (await response.Content.ReadAsync<SeedDataResponse>())!;
        await PopulateStripeIdsAsync();
    }

    private async Task PopulateStripeIdsAsync()
    {
        var customer = await ResolvePayoutAccountAsync(SeedData.Customer.Id, requiresAccount: false);
        SeedData.Customer.StripeCustomerId = customer.StripeCustomerId!;

        var venueManager = await ResolvePayoutAccountAsync(SeedData.VenueManager1.Id, requiresAccount: true);
        SeedData.VenueManager1.StripeCustomerId = venueManager.StripeCustomerId!;
        SeedData.VenueManager1.StripeAccountId = venueManager.StripeAccountId!;

        var artistManager = await ResolvePayoutAccountAsync(SeedData.ArtistManager1.Id, requiresAccount: true);
        SeedData.ArtistManager1.StripeCustomerId = artistManager.StripeCustomerId!;
        SeedData.ArtistManager1.StripeAccountId = artistManager.StripeAccountId!;
    }

    private async Task<PayoutAccountRow> ResolvePayoutAccountAsync(Guid userId, bool requiresAccount)
    {
        var account = await Polling.UntilAsync(
            () => Db.Payment.GetPayoutAccountByUserIdAsync(userId),
            row => row is not null
                && row.StripeCustomerId is not null
                && (!requiresAccount || row.StripeAccountId is not null),
            timeout: TimeSpan.FromSeconds(60),
            interval: TimeSpan.FromSeconds(1));
        return account!;
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
        loggerFactory.Dispose();
    }

    public ResourceNotificationService ResourceNotifications => app.ResourceNotifications;

    private async Task WaitForAppAsync()
    {
        logger.WaitingForAppToBeHealthy(ApiBaseUrl);

        await WaitForHealthAsync(Client);

        using var customerClient = new HttpClient { BaseAddress = new Uri(CustomerApiBaseUrl) };
        await WaitForHealthAsync(customerClient);

        logger.AppIsHealthy();
    }

    private async Task WaitForHealthAsync(HttpClient client)
    {
        await Polling.UntilAsync(async () =>
        {
            var response = await client.GetAsync("/health");
            logger.HealthCheck(response.StatusCode);
            return response.IsSuccessStatusCode;
        },
        timeout: TimeSpan.FromMinutes(3),
        interval: TimeSpan.FromSeconds(1));
    }

    private static ILoggerFactory BuildMessageSinkLoggerFactory(IMessageSink messageSink) =>
        LoggerFactory.Create(b => b
            .AddProvider(new MessageSinkLoggerProvider(messageSink))
            .SetMinimumLevel(LogLevel.Debug));
}
