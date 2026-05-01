using Concertable.Application.Interfaces;
using Concertable.Notification.Contracts;
using Concertable.Payment.Contracts;
using Concertable.User.Contracts;
using Concertable.User.Domain;
using Concertable.Payment.Application.Interfaces;
using Concertable.IntegrationTests.Common.Mocks;
using Concertable.Artist.Infrastructure.Extensions;
using Concertable.Concert.Infrastructure.Extensions;
using Concertable.Contract.Infrastructure.Extensions;
using Concertable.User.Infrastructure.Extensions;
using Concertable.Venue.Infrastructure.Extensions;
using Concertable.Payment.Infrastructure.Extensions;
using Concertable.Customer.Infrastructure.Extensions;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Payment.Infrastructure.Services;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Payment.Application.Interfaces.Webhook;
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

namespace Concertable.IntegrationTests.Common;

public class ApiFixture : IAsyncLifetime
{
    private SqlFixture sqlFixture = null!;
    private WebApplicationFactory<Program> factory = null!;
    private IServiceScope? scope;

    public IMockNotificationService NotificationService { get; } = new MockNotificationService();
    public IMockStripePaymentClient StripePaymentClient { get; } = new MockStripePaymentClient();
    public IMockEmailService EmailService { get; } = new MockEmailService();
    public IStripeClient StripeClient { get; private set; } = null!;
    public SeedData SeedData { get; private set; } = null!;
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
                    ["Urls:Frontend"] = "https://localhost:5173",
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<INotificationModule>(NotificationService);
                services.AddSingleton<IMockStripePaymentClient>(StripePaymentClient);
                services.AddSingleton<IStripePaymentClient>(StripePaymentClient);
                services.AddKeyedScoped<IStripePaymentIntentClient, OnSessionStripePaymentIntentClient>(PaymentSession.OnSession);
                services.AddKeyedScoped<IStripePaymentIntentClient, OffSessionStripePaymentIntentClient>(PaymentSession.OffSession);
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
                services.AddUserTestSeeder();
                services.AddArtistTestSeeder();
                services.AddVenueTestSeeder();
                services.AddContractTestSeeder();
                services.AddConcertTestSeeder();
                services.AddPaymentTestSeeder();
                services.AddMessagingTestSeeder();
                services.AddCustomerTestSeeder();

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
