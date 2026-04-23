using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Fakers;
using Concertable.Venue.Contracts.Events;
using Concertable.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Venue.Infrastructure.Data.Seeders;

internal class VenueDevSeeder : IDevSeeder
{
    public int Order => 2;

    private readonly VenueDbContext context;
    private readonly SeedData seed;
    private readonly IGeometryProvider geometryProvider;
    private readonly ILocationFaker locationFaker;
    private readonly IIntegrationEventBus eventBus;

    public VenueDevSeeder(
        VenueDbContext context,
        SeedData seed,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        ILocationFaker locationFaker,
        IIntegrationEventBus eventBus)
    {
        this.context = context;
        this.seed = seed;
        this.geometryProvider = geometryProvider;
        this.locationFaker = locationFaker;
        this.eventBus = eventBus;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var venueManagerIds = seed.VenueManagerIds;

        await context.Venues.SeedIfEmptyAsync(async () =>
        {
            var venues = new VenueEntity[]
            {
                VenueFaker.GetFaker(venueManagerIds[0], "The Grand Venue", "grandvenue.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[1], "Redhill Hall", "redhillhall.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[2], "Weybridge Pavilion", "weybridgepavilon.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[3], "Cobham Arts Centre", "cobhamarts.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[4], "Chertsey Arena", "chertseyarena.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[5], "Camden Electric Ballroom", "camdenballroom.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[6], "Manchester Night & Day Café", "manchesternightday.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[7], "Birmingham O2 Institute", "birminghamo2.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[8], "Edinburgh Usher Hall", "edinburghusher.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[9], "Liverpool Philharmonic Hall", "liverpoolphilharmonic.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[10], "Leeds Brudenell Social Club", "leedsbrudenell.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[11], "Glasgow Barrowland Ballroom", "glasgowbarrowland.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[12], "Sheffield Leadmill", "sheffieldleadmill.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[13], "Nottingham Rock City", "nottinghamrockcity.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[14], "Bristol Thekla", "bristolthekla.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[15], "Brighton Concorde 2", "brightonconcorde2.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[16], "Cardiff Tramshed", "cardifftramshed.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[17], "Newcastle O2 Academy", "newcastleo2.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[18], "Oxford O2 Academy", "oxfordo2.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[19], "Cambridge Corn Exchange", "cambridgecornexchange.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[20], "Bath Komedia", "bathkomedia.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[21], "Aberdeen The Lemon Tree", "aberdeenlemontree.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[22], "York Barbican", "yorkbarbican.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[23], "Belfast Limelight", "belfastlimelight.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[24], "Dublin Vicar Street", "dublinvicarstreet.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[25], "Norwich Waterfront", "norwichwaterfront.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[26], "Exeter Phoenix", "exeterphoenix.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[27], "Southampton Engine Rooms", "southamptonengine.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[28], "Hull The Welly Club", "hullwellyclub.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[29], "Plymouth Junction", "plymouthjunction.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[30], "Swansea Sin City", "swanseasincity.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[31], "Inverness Ironworks", "invernessironworks.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[32], "Stirling Albert Halls", "stirlingalberthalls.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[33], "Dundee Fat Sams", "dundeefatsams.jpg").Generate(),
                VenueFaker.GetFaker(venueManagerIds[34], "Coventry Empire", "coventryempire.jpg").Generate()
            };
            foreach (var venue in venues)
            {
                var loc = locationFaker.Next();
                venue.Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude);
                venue.Address = new Address(loc.County, loc.Town);
                venue.Email = string.Empty;
            }

            context.Venues.AddRange(venues);
            await context.SaveChangesAsync(ct);

            foreach (var venue in venues)
            {
                await eventBus.PublishAsync(new VenueChangedEvent(
                    venue.Id,
                    venue.UserId,
                    venue.Name,
                    venue.About,
                    venue.Address?.County,
                    venue.Address?.Town,
                    venue.Location?.Y,
                    venue.Location?.X), ct);
            }
        });
    }
}
