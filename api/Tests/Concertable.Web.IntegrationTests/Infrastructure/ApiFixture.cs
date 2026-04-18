using Concertable.Application.Interfaces;
using Concertable.Core.Enums;
using Concertable.Application.Interfaces.Payment;
using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Interfaces;
using Concertable.Infrastructure.Services.Payment;
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

    public IMockNotificationService NotificationService { get; } = new MockNotificationService();
    public IMockStripePaymentClient StripePaymentClient { get; } = new MockStripePaymentClient();
    public IMockEmailService EmailService { get; } = new MockEmailService();
    public IStripeClient StripeClient { get; private set; } = null!;
    public SeedData SeedData { get; private set; } = null!;

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
                services.AddResettables(NotificationService, StripePaymentClient, EmailService);
                services.AddSingleton<IEmailService>(EmailService);
                services.AddScoped<IPaymentService, PaymentService>();
                services.AddScoped<IWebhookService, MockWebhookService>();
                services.AddSingleton<IStripeClient, MockStripeClient>();
                services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory>(_ => new WebApplicationHttpClientFactory(factory)));
                services.AddScoped<IGeocodingService, MockGeocodingService>();
                services.AddScoped<IDbInitializer, TestDbInitializer>();
                services.AddScoped<SeedData>();
                services.AddScoped<ILocationFaker, LocationFaker>();

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
        await factory.DisposeAsync();
        await sqlFixture.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        await sqlFixture.ResetAsync();
        foreach (var resettable in factory.Services.GetServices<IResettable>())
            resettable.Reset();
        StripeClient = factory.Services.GetRequiredService<IStripeClient>();

        using var scope = factory.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.InitializeAsync();
        SeedData = scope.ServiceProvider.GetRequiredService<SeedData>();
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

    public HttpClient CreateClient() => factory.CreateClient();
}
