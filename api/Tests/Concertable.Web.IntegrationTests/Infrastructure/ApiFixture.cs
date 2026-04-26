using Concertable.Application.Interfaces;
using Concertable.Core.Enums;
using Concertable.Payment.Application.Interfaces;
using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Concertable.Artist.Infrastructure.Extensions;
using Concertable.Concert.Infrastructure.Extensions;
using Concertable.Contract.Infrastructure.Extensions;
using Concertable.Identity.Infrastructure.Extensions;
using Concertable.Venue.Infrastructure.Extensions;
using Concertable.Payment.Infrastructure.Extensions;
using Concertable.Payment.Infrastructure.Services;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Infrastructure.Data;
using Concertable.Payment.Application.Interfaces.Webhook;
using Concertable.Payment.Application.Interfaces;
using Concertable.Seeding;
using Concertable.Seeding.Fakers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class ApiFixture : IAsyncLifetime
{
    private SqlFixture sqlFixture = null!;
    private WebApplicationFactory<Program> factory = null!;
    private IServiceScope? scope;

    internal IMockNotificationService NotificationService { get; } = new MockNotificationService();
    internal IMockStripePaymentClient StripePaymentClient { get; } = new MockStripePaymentClient();
    public IMockEmailService EmailService { get; } = new MockEmailService();
    public IStripeClient StripeClient { get; private set; } = null!;
    public SeedData SeedData { get; private set; } = null!;
    public ApplicationDbContext DbContext { get; private set; } = null!;
    public IReadDbContext ReadDbContext { get; private set; } = null!;

public async Task InitializeAsync()
    {
        sqlFixture = new SqlFixture();
        await sqlFixture.InitializeAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = sqlFixture.ConnectionString,
                    ["ExternalServices:UseRealStripe"] = "false",
                    ["ExternalServices:UseRealBlob"] = "false",
                    ["ExternalServices:UseRealEmail"] = "false",
                    ["Auth:JwtSigningKeyBase64"] = Convert.ToBase64String(new byte[32]),
                    ["Auth:Issuer"] = "test",
                    ["Auth:Audience"] = "test",
                    ["Urls:Frontend"] = "https://localhost:5173",
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IConcertNotificationService>(NotificationService);
                services.AddSingleton<IApplicationNotificationService>(NotificationService);
                services.AddSingleton<ITicketNotificationService>(NotificationService);
                services.AddSingleton<IMockStripePaymentClient>(StripePaymentClient);
                services.AddSingleton<IStripePaymentClient>(StripePaymentClient);
                services.AddKeyedScoped<IPaymentService, OnSessionPaymentService>(PaymentSession.OnSession);
                services.AddKeyedScoped<IPaymentService, OffSessionPaymentService>(PaymentSession.OffSession);
                services.AddResettables(NotificationService, StripePaymentClient, EmailService);
                services.AddSingleton<IEmailService>(EmailService);

                services.AddScoped<IWebhookService, MockWebhookService>();
                services.AddSingleton<IStripeClient, MockStripeClient>();
                services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory>(_ => new WebApplicationHttpClientFactory(factory)));
                services.AddScoped<IGeocodingService, MockGeocodingService>();
                services.AddScoped<IDbInitializer, TestDbInitializer>();
                services.AddScoped<SeedData>();
                services.AddScoped<ILocationFaker, LocationFaker>();
                services.AddSharedTestSeeder();
                services.AddIdentityTestSeeder();
                services.AddArtistTestSeeder();
                services.AddVenueTestSeeder();
                services.AddContractTestSeeder();
                services.AddConcertTestSeeder();
                services.AddPaymentTestSeeder();

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    opts.DefaultScheme = TestAuthHandler.SchemeName;
                });
                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            });
        });

        _ = factory.Services;

        await sqlFixture.InitializeRespawnerAsync();
        StripeClient = factory.Services.GetRequiredService<IStripeClient>();
    }

    public async Task DisposeAsync()
    {
        scope?.Dispose();
        await factory.DisposeAsync();
        await sqlFixture.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        await sqlFixture.ResetAsync();
        foreach (var resettable in factory.Services.GetServices<IResettable>())
            resettable.Reset();
        StripeClient = factory.Services.GetRequiredService<IStripeClient>();

        scope?.Dispose();
        scope = factory.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.InitializeAsync();
        SeedData = scope.ServiceProvider.GetRequiredService<SeedData>();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ReadDbContext = scope.ServiceProvider.GetRequiredService<IReadDbContext>();
    }

    public HttpClient CreateClient(UserEntity user)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        return client;
    }

    public HttpClient CreateClient(UserEntity user, Action<TestClientOptions> configure)
    {
        var options = new TestClientOptions();
        configure(options);

        var customFactory = factory.WithWebHostBuilder(b =>
        {
            if (options.Configure is not null)
                b.ConfigureAppConfiguration((_, config) => options.Configure(config));
            if (options.Services is not null)
                b.ConfigureTestServices(options.Services);
        });

        StripeClient = customFactory.Services.GetRequiredService<IStripeClient>();

        var client = customFactory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        return client;
    }

    public HttpClient CreateClient(Guid userId, Role role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role.ToString());
        return client;
    }

    public HttpClient CreateClient(Guid userId, Role role, Action<TestClientOptions> configure)
    {
        var options = new TestClientOptions();
        configure(options);

        var customFactory = factory.WithWebHostBuilder(b =>
        {
            if (options.Configure is not null)
                b.ConfigureAppConfiguration((_, config) => options.Configure(config));
            if (options.Services is not null)
                b.ConfigureTestServices(options.Services);
        });

        StripeClient = customFactory.Services.GetRequiredService<IStripeClient>();

        var client = customFactory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role.ToString());
        return client;
    }

    public HttpClient CreateClient() => factory.CreateClient();
}
