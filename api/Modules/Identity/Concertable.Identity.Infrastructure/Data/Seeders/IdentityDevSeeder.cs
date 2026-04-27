using Concertable.Application.Interfaces.Geometry;
using Concertable.Shared.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Identity.Infrastructure.Data.Seeders;

internal class IdentityDevSeeder : IDevSeeder
{
    public int Order => 0;

    private readonly IdentityDbContext context;
    private readonly SeedData seedData;
    private readonly IPasswordHasher passwordHasher;
    private readonly IGeometryProvider geometryProvider;
    private readonly ILocationFaker locationFaker;

    public IdentityDevSeeder(
        IdentityDbContext context,
        SeedData seedData,
        IPasswordHasher passwordHasher,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        ILocationFaker locationFaker)
    {
        this.context = context;
        this.seedData = seedData;
        this.passwordHasher = passwordHasher;
        this.geometryProvider = geometryProvider;
        this.locationFaker = locationFaker;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Users.SeedIfEmptyAsync(async () =>
        {
            var hash = passwordHasher.Hash(SeedData.TestPassword);

            seedData.Admin = UserFactory.Admin("admin@test.com", hash);
            seedData.Admin.UpdateLocation(geometryProvider.CreatePoint(51.0, -0.5), new Address("Leicestershire", "Loughborough"));
            seedData.Admin.UpdateAvatar("avatar.jpg");
            context.Users.Add(seedData.Admin);

            var customerLoc = locationFaker.Next();
            seedData.Customer = UserFactory.Customer("customer1@test.com", hash);
            seedData.Customer.UpdateLocation(geometryProvider.CreatePoint(customerLoc.Latitude, customerLoc.Longitude), new Address(customerLoc.County, customerLoc.Town));
            seedData.Customer.UpdateAvatar("avatar.jpg");
            context.Users.Add(seedData.Customer);

            for (int i = 2; i <= 6; i++)
            {
                var loc = locationFaker.Next();
                var c = UserFactory.Customer($"customer{i}@test.com", hash);
                c.UpdateLocation(geometryProvider.CreatePoint(loc.Latitude, loc.Longitude), new Address(loc.County, loc.Town));
                c.UpdateAvatar("avatar.jpg");
                context.Users.Add(c);
            }

            seedData.ArtistManager = UserFactory.ArtistManager("artistmanager1@test.com", hash);
            context.Users.Add(seedData.ArtistManager);

            var artistManager2 = UserFactory.ArtistManager("artistmanager2@test.com", hash);
            context.Users.Add(artistManager2);

            for (int i = 3; i <= 35; i++)
                context.Users.Add(UserFactory.ArtistManager($"artistmanager{i}@test.com", hash));

            seedData.VenueManager1 = UserFactory.VenueManager("venuemanager1@test.com", hash);
            context.Users.Add(seedData.VenueManager1);

            seedData.VenueManager2 = UserFactory.VenueManager("venuemanager2@test.com", hash);
            context.Users.Add(seedData.VenueManager2);

            for (int i = 3; i <= 35; i++)
                context.Users.Add(UserFactory.VenueManager($"venuemanager{i}@test.com", hash));

            await context.SaveChangesAsync(ct);
        });

        // Always resolve IDs from DB so downstream seeders work on first run and restarts
        var usersByEmail = await context.Users.ToDictionaryAsync(u => u.Email, u => u.Id, ct);

        var customerIds = new List<Guid> { usersByEmail["customer1@test.com"] };
        for (int i = 2; i <= 6; i++) customerIds.Add(usersByEmail[$"customer{i}@test.com"]);
        seedData.CustomerIds = customerIds;

        var artistManagerIds = new List<Guid>();
        for (int i = 1; i <= 35; i++) artistManagerIds.Add(usersByEmail[$"artistmanager{i}@test.com"]);
        seedData.ArtistManagerIds = artistManagerIds;

        var venueManagerIds = new List<Guid> { usersByEmail["venuemanager1@test.com"], usersByEmail["venuemanager2@test.com"] };
        for (int i = 3; i <= 35; i++) venueManagerIds.Add(usersByEmail[$"venuemanager{i}@test.com"]);
        seedData.VenueManagerIds = venueManagerIds;
    }
}
