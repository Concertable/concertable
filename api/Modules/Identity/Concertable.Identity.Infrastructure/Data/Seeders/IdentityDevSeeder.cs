using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Services.Geometry;
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
            seedData.Admin.Avatar = "avatar.jpg";
            context.Users.Add(seedData.Admin);

            var customerLoc = locationFaker.Next();
            seedData.Customer = UserFactory.Customer("customer1@test.com", hash);
            seedData.Customer.UpdateLocation(geometryProvider.CreatePoint(customerLoc.Latitude, customerLoc.Longitude), new Address(customerLoc.County, customerLoc.Town));
            seedData.Customer.Avatar = "avatar.jpg";
            context.Users.Add(seedData.Customer);

            for (int i = 2; i <= 6; i++)
            {
                var loc = locationFaker.Next();
                var c = UserFactory.Customer($"customer{i}@test.com", hash);
                c.UpdateLocation(geometryProvider.CreatePoint(loc.Latitude, loc.Longitude), new Address(loc.County, loc.Town));
                c.Avatar = "avatar.jpg";
                context.Users.Add(c);
            }

            var am1Loc = locationFaker.Next();
            seedData.ArtistManager = UserFactory.ArtistManager("artistmanager1@test.com", hash);
            seedData.ArtistManager.UpdateLocation(geometryProvider.CreatePoint(am1Loc.Latitude, am1Loc.Longitude), new Address(am1Loc.County, am1Loc.Town));
            seedData.ArtistManager.Avatar = "avatar.jpg";
            context.Users.Add(seedData.ArtistManager);

            var am2Loc = locationFaker.Next();
            var artistManager2 = UserFactory.ArtistManager("artistmanager2@test.com", hash);
            artistManager2.UpdateLocation(geometryProvider.CreatePoint(am2Loc.Latitude, am2Loc.Longitude), new Address(am2Loc.County, am2Loc.Town));
            artistManager2.Avatar = "avatar.jpg";
            context.Users.Add(artistManager2);

            for (int i = 3; i <= 35; i++)
            {
                var loc = locationFaker.Next();
                var am = UserFactory.ArtistManager($"artistmanager{i}@test.com", hash);
                am.UpdateLocation(geometryProvider.CreatePoint(loc.Latitude, loc.Longitude), new Address(loc.County, loc.Town));
                am.Avatar = "avatar.jpg";
                context.Users.Add(am);
            }

            var vm1Loc = locationFaker.Next();
            seedData.VenueManager1 = UserFactory.VenueManager("venuemanager1@test.com", hash);
            seedData.VenueManager1.UpdateLocation(geometryProvider.CreatePoint(vm1Loc.Latitude, vm1Loc.Longitude), new Address(vm1Loc.County, vm1Loc.Town));
            seedData.VenueManager1.Avatar = "avatar.jpg";
            context.Users.Add(seedData.VenueManager1);

            var vm2Loc = locationFaker.Next();
            seedData.VenueManager2 = UserFactory.VenueManager("venuemanager2@test.com", hash);
            seedData.VenueManager2.UpdateLocation(geometryProvider.CreatePoint(vm2Loc.Latitude, vm2Loc.Longitude), new Address(vm2Loc.County, vm2Loc.Town));
            seedData.VenueManager2.Avatar = "avatar.jpg";
            context.Users.Add(seedData.VenueManager2);

            for (int i = 3; i <= 35; i++)
            {
                var loc = locationFaker.Next();
                var vm = UserFactory.VenueManager($"venuemanager{i}@test.com", hash);
                vm.UpdateLocation(geometryProvider.CreatePoint(loc.Latitude, loc.Longitude), new Address(loc.County, loc.Town));
                vm.Avatar = "avatar.jpg";
                context.Users.Add(vm);
            }

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
