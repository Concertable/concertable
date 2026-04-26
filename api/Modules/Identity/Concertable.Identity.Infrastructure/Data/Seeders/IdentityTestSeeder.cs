using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;
using Concertable.Seeding.Factories;
using Concertable.Seeding.Fakers;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Identity.Infrastructure.Data.Seeders;

internal class IdentityTestSeeder : ITestSeeder
{
    public int Order => 0;

    private readonly IdentityDbContext context;
    private readonly SeedData seedData;
    private readonly IPasswordHasher passwordHasher;
    private readonly IGeometryProvider geometryProvider;
    private readonly ILocationFaker locationFaker;

    public IdentityTestSeeder(
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

            var vm1Loc = locationFaker.Next();
            seedData.VenueManager1 = UserFactory.VenueManager("venuemanager1@test.com", hash);
            seedData.VenueManager1.UpdateLocation(geometryProvider.CreatePoint(vm1Loc.Latitude, vm1Loc.Longitude), new Address(vm1Loc.County, vm1Loc.Town));

            var vm2Loc = locationFaker.Next();
            seedData.VenueManager2 = UserFactory.VenueManager("venuemanager2@test.com", hash);
            seedData.VenueManager2.UpdateLocation(geometryProvider.CreatePoint(vm2Loc.Latitude, vm2Loc.Longitude), new Address(vm2Loc.County, vm2Loc.Town));

            seedData.ArtistManager = UserFactory.ArtistManager("artistmanager1@test.com", hash);
            seedData.ArtistManager.UpdateLocation(geometryProvider.CreatePoint(51, 0), new Address("Test County", "Test Town"));

            seedData.ArtistManagerNoArtist = UserFactory.ArtistManager("artistmanager2@test.com", hash);
            seedData.ArtistManagerNoArtist.UpdateLocation(geometryProvider.CreatePoint(51, 0), new Address("Test County", "Test Town"));

            seedData.Customer = UserFactory.Customer("customer@test.com", hash);
            seedData.Customer.UpdateLocation(geometryProvider.CreatePoint(51, 0));

            seedData.Admin = UserFactory.Admin("admin@test.com", hash);
            seedData.Admin.UpdateLocation(geometryProvider.CreatePoint(51, 0));

            context.Users.AddRange(
                seedData.VenueManager1,
                seedData.VenueManager2,
                seedData.ArtistManager,
                seedData.ArtistManagerNoArtist,
                seedData.Customer,
                seedData.Admin);

            await context.SaveChangesAsync(ct);
        });
    }
}
