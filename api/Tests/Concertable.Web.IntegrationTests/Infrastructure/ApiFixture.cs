using Application.Interfaces;
using Application.Interfaces.Payment;
using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Infrastructure.Interfaces;
using Infrastructure.Services.Payment;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class ApiFixture : IAsyncLifetime
{
    private readonly SqlFixture sqlFixture = new();
    private WebApplicationFactory<Program> factory = null!;

    public MockNotificationService NotificationService { get; } = new();
    public MockStripePaymentClient StripeClient { get; } = new();
    public FakeStripeClient FakeStripeClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await sqlFixture.InitializeAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
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
                services.AddSingleton<IStripePaymentClient>(StripeClient);
                services.AddScoped<IPaymentService, PaymentService>();
                services.AddScoped<IWebhookService, MockWebhookService>();
                services.AddScoped<TestDbInitializer>();

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

        using var scope = factory.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<TestDbInitializer>();
        await initializer.InitializeAsync();

        await sqlFixture.InitializeRespawnerAsync();
        FakeStripeClient = new FakeStripeClient(StripeClient, new WebApplicationHttpClientFactory(factory));
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

        using var scope = factory.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<TestDbInitializer>();
        await initializer.SeedApplicationAsync();
    }

    public HttpClient CreateClient(TestUser user)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        return client;
    }

    public HttpClient CreateClient(TestUser user, Action<IServiceCollection> configure)
    {
        var client = factory.WithWebHostBuilder(b => b.ConfigureTestServices(configure)).CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        return client;
    }

    public HttpClient CreateClient() => factory.CreateClient();

    public HttpClient CreateClient(Action<IServiceCollection> configure) =>
        factory.WithWebHostBuilder(b => b.ConfigureTestServices(configure)).CreateClient();
}
