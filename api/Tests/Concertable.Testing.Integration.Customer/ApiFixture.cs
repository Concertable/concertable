using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Messaging;
using Concertable.Payment.Client;
using Concertable.Seeding;
using Concertable.Shared;
using Concertable.Shared.Email;
using Concertable.Testing.Integration;
using Concertable.Testing.Integration.Mocks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Concertable.Testing.Integration.Customer;

public class ApiFixture : IAsyncLifetime
{
    private SqlFixture sqlFixture = null!;
    private WebApplicationFactory<Program> factory = null!;

    public SeedCustomer Customer => SeedCustomers.Customer1;
    public SeedCustomer OtherCustomer => SeedCustomers.Customer2;

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
                    ["ConnectionStrings:CustomerDb"] = sqlFixture.ConnectionString,
                });
            });

            builder.ConfigureTestServices(services =>
            {
                var asbDescriptors = services
                    .Where(d => d.ServiceType == typeof(IHostedService) &&
                                d.ImplementationType?.Name == "AzureServiceBusReceiver")
                    .ToList();
                foreach (var d in asbDescriptors)
                    services.Remove(d);

                services.Replace(ServiceDescriptor.Singleton<IBusTransport, MockBusTransport>());
                services.AddScoped<ICustomerPaymentClient, MockCustomerPaymentClient>();
                services.AddSingleton<IEmailSender, MockEmailSender>();

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
    }

    public async Task ResetAsync() => await sqlFixture.ResetAsync();

    public async Task DisposeAsync()
    {
        await factory.DisposeAsync();
        await sqlFixture.DisposeAsync();
    }

    public HttpClient CreateClient() => factory.CreateClient();

    public HttpClient CreateClient(SeedCustomer customer)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, customer.Id.ToString());
        return client;
    }

    public IServiceProvider Services => factory.Services;

    public async Task<Concertable.Customer.User.Domain.UserEntity> SeedUserAsync(SeedCustomer customer)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var user = Concertable.Customer.User.Domain.UserEntity.FromRegistration(customer.Id, customer.Email);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<Concertable.Customer.Venue.Domain.VenueReadModel> SeedVenueAsync(int id = 1)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VenueDbContext>();
        var venue = Concertable.Customer.Venue.Domain.VenueReadModel.Create(
            id, Guid.NewGuid(), "Test Venue", "About the venue", "avatar.jpg", "banner.jpg",
            "Test County", "Test Town", 51.5, -0.1, "venue@test.com");
        db.Venues.Add(venue);
        await db.SaveChangesAsync();
        return venue;
    }

    public async Task<Concertable.Customer.Artist.Domain.ArtistReadModel> SeedArtistAsync(int id = 1)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ArtistDbContext>();
        var artist = Concertable.Customer.Artist.Domain.ArtistReadModel.Create(
            id, Guid.NewGuid(), "Test Artist", "About the artist", "avatar.jpg", "banner.jpg",
            "Test County", "Test Town", 51.5, -0.1, "artist@test.com");
        db.Artists.Add(artist);
        await db.SaveChangesAsync();
        return artist;
    }

    public async Task<Concertable.Customer.Concert.Domain.ConcertReadModel> SeedConcertAsync(
        int id = 1,
        bool posted = true,
        int availableTickets = 100,
        DateTime? startDate = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        var start = startDate ?? DateTime.UtcNow.AddDays(7);
        var period = new DateRange(start, start.AddHours(3));
        var concert = Concertable.Customer.Concert.Domain.ConcertReadModel.Create(
            id, "Test Concert", "About the concert",
            null, null, availableTickets, 15.00m, period,
            posted ? DateTime.UtcNow.AddDays(-1) : null,
            artistId: 1, "Test Artist", venueId: 1, "Test Venue",
            Guid.NewGuid(), "FlatFee");
        db.Concerts.Add(concert);
        await db.SaveChangesAsync();
        return concert;
    }

    public async Task<Concertable.Customer.Ticket.Domain.TicketEntity> SeedTicketAsync(
        Guid userId,
        int concertId,
        bool upcoming = true)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        var concertStart = upcoming ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(-7);
        var period = new DateRange(concertStart, concertStart.AddHours(3));
        var ticket = Concertable.Customer.Ticket.Domain.TicketEntity.Create(
            Guid.CreateVersion7(), userId, concertId,
            [], DateTime.UtcNow,
            "Test Concert", 15.00m, period, "Test Venue", "Test Artist");
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return ticket;
    }
}
