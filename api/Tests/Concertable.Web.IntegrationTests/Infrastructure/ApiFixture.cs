using Application.Interfaces;
using Application.Interfaces.Payment;
using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Infrastructure.Interfaces;
using Infrastructure.Services.Payment;
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
    private readonly SqlFixture sqlFixture = new();
    private WebApplicationFactory<Program> factory = null!;

    public MockNotificationService NotificationService { get; } = new();
    public MockStripePaymentClient StripeClient { get; } = new();
    public IFakeStripeClient FakeStripeClient { get; private set; } = null!;

public async Task InitializeAsync()
    {
        await sqlFixture.InitializeAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = sqlFixture.ConnectionString,
                    ["UseFakeExternalServices"] = "true",
                    ["Auth:JwtSigningKeyBase64"] = Convert.ToBase64String(new byte[32]),
                    ["Auth:Issuer"] = "test",
                    ["Auth:Audience"] = "test",
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IConcertNotificationService>(NotificationService);
                services.AddSingleton<ITicketNotificationService>(NotificationService);
                services.AddScoped<IEmailService, MockEmailService>();
                services.AddSingleton(StripeClient);
                services.AddSingleton<IStripePaymentClient>(StripeClient);
                services.AddScoped<IPaymentService, PaymentService>();
                services.AddScoped<IWebhookService, MockWebhookService>();
                services.AddSingleton<IFakeStripeClient, FakeStripeClient>();
                services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory>(_ => new WebApplicationHttpClientFactory(factory)));
                services.AddScoped<IDbInitializer, TestDbInitializer>();

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
        FakeStripeClient = factory.Services.GetRequiredService<IFakeStripeClient>();
    }

    public async Task DisposeAsync()
    {
        await factory.DisposeAsync();
        await sqlFixture.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        await sqlFixture.ResetAsync();
        NotificationService.Reset();
        StripeClient.Reset();
        FakeStripeClient = factory.Services.GetRequiredService<IFakeStripeClient>();

        using var scope = factory.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.InitializeAsync();
    }

    public HttpClient CreateClient(TestUser user)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        return client;
    }

    public HttpClient CreateClient(TestUser user, Action<TestClientOptions> configure)
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

        FakeStripeClient = customFactory.Services.GetRequiredService<IFakeStripeClient>();

        var client = customFactory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        return client;
    }

    public HttpClient CreateClient() => factory.CreateClient();
}
