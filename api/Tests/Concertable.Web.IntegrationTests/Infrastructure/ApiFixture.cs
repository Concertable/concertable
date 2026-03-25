using Application.Interfaces;
using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class ApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer container = new MsSqlBuilder().Build();
    private WebApplicationFactory<Program> factory = null!;

    public MockNotificationService NotificationService { get; } = new();

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = container.GetConnectionString(),
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
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await TestDbInitializer.InitializeAsync(db);
    }

    public async Task DisposeAsync()
    {
        await factory.DisposeAsync();
        await container.DisposeAsync();
    }

    public HttpClient CreateClient(Guid userId, string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    public HttpClient CreateClient(Guid userId, string role, Action<IServiceCollection> configure)
    {
        var client = factory.WithWebHostBuilder(b => b.ConfigureTestServices(configure)).CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    public HttpClient CreateClient() => factory.CreateClient();

    public HttpClient CreateClient(Action<IServiceCollection> configure) =>
        factory.WithWebHostBuilder(b => b.ConfigureTestServices(configure)).CreateClient();
}
