using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Concertable.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Net.Http.Headers;

namespace Concertable.E2ETests;

public class AppFixture : IAsyncLifetime
{
    private static readonly HttpClient healthClient = new(
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                message.RequestUri?.IsLoopback == true
        });

    private DistributedApplication app = null!;
    private AspireResourceLogger resourceLogger = null!;
    public ILoggerFactory LoggerFactory { get; }
    private readonly ILogger<AppFixture> logger;
    private readonly IConfiguration configuration;
    private readonly TestTokenMinter tokenMinter;

    public const string TestPaymentMethodId = "pm_card_visa";

    public string B2BWebUrl { get; }
    public string CustomerWebUrl { get; }
    public string SearchWebUrl { get; }
    public string PaymentWebUrl { get; }
    public string AuthUrl { get; }
    public string CustomerSpaUrl { get; }
    public string VenueSpaUrl { get; }
    public string ArtistSpaUrl { get; }
    public string BusinessSpaUrl { get; }
    public HttpClient B2BClient { get; private set; } = null!;
    public HttpClient CustomerClient { get; private set; } = null!;
    public HttpClient SearchClient { get; private set; } = null!;
    public HttpClient PaymentClient { get; private set; } = null!;
    public IPollingService Polling { get; private set; } = null!;
    public PaymentIntentService StripePaymentIntents { get; private set; } = null!;
    public StripeFixture Stripe { get; private set; } = null!;
    public SeedDataResponse SeedData { get; private set; } = null!;
    public SqlFixture Sql { get; private set; } = null!;
    public TestDb Db { get; private set; } = null!;

    public AppFixture()
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => b
            .AddSimpleConsole(o => o.SingleLine = true)
            .SetMinimumLevel(LogLevel.Information));
        logger = LoggerFactory.CreateLogger<AppFixture>();
        Polling = new PollingService(LoggerFactory.CreateLogger<PollingService>());

        configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.E2E.json"))
            .AddEnvironmentVariables()
            .Build();

        var endpoints = configuration.GetSection("Endpoints").Get<E2EEndpoints>()
            ?? throw new InvalidOperationException("Endpoints section is missing from appsettings.E2E.json.");

        B2BWebUrl      = endpoints.B2BWeb;
        CustomerWebUrl = endpoints.CustomerWeb;
        SearchWebUrl   = endpoints.SearchWeb;
        PaymentWebUrl  = endpoints.PaymentWeb;
        AuthUrl        = endpoints.Auth;
        CustomerSpaUrl = endpoints.CustomerSpa;
        VenueSpaUrl    = endpoints.VenueSpa;
        ArtistSpaUrl   = endpoints.ArtistSpa;
        BusinessSpaUrl = endpoints.BusinessSpa;

        tokenMinter = new TestTokenMinter(configuration);
    }

    public async Task InitializeAsync()
    {
        logger.InitializingE2ETestFixture();

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Concertable_AppHost>();

        builder.AddE2E(B2BWebUrl, CustomerWebUrl, SearchWebUrl, AuthUrl, PaymentWebUrl);
        var stripeClient = new StripeClient(configuration["Stripe:SecretKey"]);
        StripePaymentIntents = new PaymentIntentService(stripeClient);
        Stripe = new StripeFixture(stripeClient);

        app = await builder.BuildAsync();
        resourceLogger = new AspireResourceLogger(app.ResourceNotifications, logger);
        await app.StartAsync();

        B2BClient      = new HttpClient();
        CustomerClient = new HttpClient();
        SearchClient   = new HttpClient();
        PaymentClient  = new HttpClient();

        await WaitForAppAsync();

        Sql = new SqlFixture();
        await Sql.InitializeAsync(app);
        Db = new TestDb(Sql.Connection, Sql.PaymentConnection);

        logger.E2ETestFixtureReady();
    }

    public async Task ResetAsync()
    {
        logger.ResettingTestState();
        Stripe.Reset();
        await Sql.ResetAsync();
        var response = await B2BClient.PostAsync($"{B2BWebUrl}/e2e/reseed");
        if (!response.IsSuccessStatusCode)
            logger.ReseedEndpointFailed((int)response.StatusCode);
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
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task DisposeAsync()
    {
        B2BClient.Dispose();
        CustomerClient.Dispose();
        SearchClient.Dispose();
        PaymentClient.Dispose();
        tokenMinter.Dispose();
        await Sql.DisposeAsync();
        await app.DisposeAsync();
        await resourceLogger.DisposeAsync();
        LoggerFactory.Dispose();
    }

    public ResourceNotificationService ResourceNotifications => app.ResourceNotifications;

    private async Task WaitForAppAsync()
    {
        logger.WaitingForAppToBeHealthy(B2BWebUrl);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(6));

        await Task.WhenAll(
            WaitForHealthAsync(B2BWebUrl, cts.Token),
            WaitForHealthAsync(CustomerWebUrl, cts.Token),
            WaitForHealthAsync(SearchWebUrl, cts.Token),
            WaitForHealthAsync(PaymentWebUrl, cts.Token));

        logger.AppIsHealthy();
    }

    private async Task WaitForHealthAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var url = $"{baseUrl}/health";

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await healthClient.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode) return;
                logger.HealthCheckError(url, $"HTTP {(int)response.StatusCode}");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.HealthCheckError(url, ex.Message);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        throw new TimeoutException($"Health check timed out for {url}");
    }

}
