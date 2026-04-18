using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.Parameters;
using Concertable.Core.ValueObjects;
using Concertable.Seeding.Fakers;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Data;

public class DevDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly SeedData seedData;
    private readonly TimeProvider timeProvider;
    private readonly IGeometryProvider geometryProvider;
    private readonly IPasswordHasher passwordHasher;
    private readonly ILocationFaker locationFaker;

    public DevDbInitializer(
        ApplicationDbContext context,
        SeedData seedData,
        TimeProvider timeProvider,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        IPasswordHasher passwordHasher,
        ILocationFaker locationFaker)
    {
        this.context = context;
        this.seedData = seedData;
        this.timeProvider = timeProvider;
        this.geometryProvider = geometryProvider;
        this.passwordHasher = passwordHasher;
        this.locationFaker = locationFaker;
    }

    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        await context.Users.SeedIfEmptyAsync(async () =>
        {
            var hash = passwordHasher.Hash(SeedData.TestPassword);

            seedData.Admin = AdminEntity.Create("admin@concertable.com", hash);
            seedData.Admin.VerifyEmail();
            seedData.Admin.Location = geometryProvider.CreatePoint(51.0, -0.5);
            seedData.Admin.Address = new Address("Leicestershire", "Loughborough");
            seedData.Admin.Avatar = "avatar.jpg";
            context.Users.Add(seedData.Admin);

            var customerLoc = locationFaker.Next();
            seedData.Customer = CustomerEntity.Create("customer@concertable.com", hash);
            seedData.Customer.VerifyEmail();
            seedData.Customer.StripeCustomerId = "cus_UIIy9Gbwfr3uAP";
            seedData.Customer.Location = geometryProvider.CreatePoint(customerLoc.Latitude, customerLoc.Longitude);
            seedData.Customer.Address = new Address(customerLoc.County, customerLoc.Town);
            seedData.Customer.Avatar = "avatar.jpg";
            context.Users.Add(seedData.Customer);

            for (int i = 2; i <= 6; i++)
            {
                var loc = locationFaker.Next();
                var c = CustomerEntity.Create($"customer{i}@concertable.com", hash);
                c.VerifyEmail();
                c.Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude);
                c.Address = new Address(loc.County, loc.Town);
                c.Avatar = "avatar.jpg";
                context.Users.Add(c);
            }

            var am1Loc = locationFaker.Next();
            seedData.ArtistManager = ArtistManagerEntity.Create("artistmanager1@concertable.com", hash);
            seedData.ArtistManager.VerifyEmail();
            seedData.ArtistManager.StripeAccountId = "acct_1TJiMePysoXmht10";
            seedData.ArtistManager.StripeCustomerId = "cus_UIIy5mCilBtJbR";
            seedData.ArtistManager.Location = geometryProvider.CreatePoint(am1Loc.Latitude, am1Loc.Longitude);
            seedData.ArtistManager.Address = new Address(am1Loc.County, am1Loc.Town);
            seedData.ArtistManager.Avatar = "avatar.jpg";
            context.Users.Add(seedData.ArtistManager);

            var am2Loc = locationFaker.Next();
            var artistManager2 = ArtistManagerEntity.Create("artistmanager2@concertable.com", hash);
            artistManager2.VerifyEmail();
            artistManager2.StripeAccountId = "acct_1TJiMoPupFslP2qz";
            artistManager2.StripeCustomerId = "cus_UIIy5415r69RmJ";
            artistManager2.Location = geometryProvider.CreatePoint(am2Loc.Latitude, am2Loc.Longitude);
            artistManager2.Address = new Address(am2Loc.County, am2Loc.Town);
            artistManager2.Avatar = "avatar.jpg";
            context.Users.Add(artistManager2);

            for (int i = 3; i <= 35; i++)
            {
                var loc = locationFaker.Next();
                var am = ArtistManagerEntity.Create($"artistmanager{i}@concertable.com", hash);
                am.VerifyEmail();
                am.Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude);
                am.Address = new Address(loc.County, loc.Town);
                am.Avatar = "avatar.jpg";
                context.Users.Add(am);
            }

            var vm1Loc = locationFaker.Next();
            seedData.VenueManager1 = VenueManagerEntity.Create("venuemanager1@concertable.com", hash);
            seedData.VenueManager1.VerifyEmail();
            seedData.VenueManager1.StripeAccountId = "acct_1TJiMjLxk4aCq1Ui";
            seedData.VenueManager1.StripeCustomerId = "cus_UIIymKfHijbNVO";
            seedData.VenueManager1.Location = geometryProvider.CreatePoint(vm1Loc.Latitude, vm1Loc.Longitude);
            seedData.VenueManager1.Address = new Address(vm1Loc.County, vm1Loc.Town);
            seedData.VenueManager1.Avatar = "avatar.jpg";
            context.Users.Add(seedData.VenueManager1);

            var vm2Loc = locationFaker.Next();
            seedData.VenueManager2 = VenueManagerEntity.Create("venuemanager2@concertable.com", hash);
            seedData.VenueManager2.VerifyEmail();
            seedData.VenueManager2.StripeAccountId = "acct_1TJiPJLLwGSDilbV";
            seedData.VenueManager2.StripeCustomerId = "cus_UIJ1qfgxYu624Q";
            seedData.VenueManager2.Location = geometryProvider.CreatePoint(vm2Loc.Latitude, vm2Loc.Longitude);
            seedData.VenueManager2.Address = new Address(vm2Loc.County, vm2Loc.Town);
            seedData.VenueManager2.Avatar = "avatar.jpg";
            context.Users.Add(seedData.VenueManager2);

            for (int i = 3; i <= 35; i++)
            {
                var loc = locationFaker.Next();
                var vm = VenueManagerEntity.Create($"venuemanager{i}@concertable.com", hash);
                vm.VerifyEmail();
                vm.Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude);
                vm.Address = new Address(loc.County, loc.Town);
                vm.Avatar = "avatar.jpg";
                context.Users.Add(vm);
            }

            await context.SaveChangesAsync();
        });

        // Always resolve IDs from DB so subsequent blocks work on first run and restarts
        var usersByEmail = await context.Users.ToDictionaryAsync(u => u.Email, u => u.Id);
        var customerIds = new List<Guid> { usersByEmail["customer@concertable.com"] };
        for (int i = 2; i <= 6; i++) customerIds.Add(usersByEmail[$"customer{i}@concertable.com"]);
        var artistManagerIds = new List<Guid>();
        for (int i = 1; i <= 35; i++) artistManagerIds.Add(usersByEmail[$"artistmanager{i}@concertable.com"]);
        var venueManagerIds = new List<Guid> { usersByEmail["venuemanager1@concertable.com"], usersByEmail["venuemanager2@concertable.com"] };
        for (int i = 3; i <= 35; i++) venueManagerIds.Add(usersByEmail[$"venuemanager{i}@concertable.com"]);

        await context.Genres.SeedIfEmptyAsync(async () =>
        {
            var genres = new GenreEntity[]
            {
                new GenreEntity { Name = "Rock" },
                new GenreEntity { Name = "Pop" },
                new GenreEntity { Name = "Jazz" },
                new GenreEntity { Name = "Hip-Hop" },
                new GenreEntity { Name = "Electronic" },
                new GenreEntity { Name = "Indie" },
                new GenreEntity { Name = "DnB" },
                new GenreEntity { Name = "House" }
            };
            context.Genres.AddRange(genres);
            await context.SaveChangesAsync();
        });

        await context.Preferences.SeedIfEmptyAsync(async () =>
        {
            var preferences = new PreferenceEntity[]
            {
                PreferenceEntity.Create(customerIds[0], 10, []),
                PreferenceEntity.Create(customerIds[1], 25, [1, 2]),
                PreferenceEntity.Create(customerIds[2], 50, [3]),
            };
            context.Preferences.AddRange(preferences);
            await context.SaveChangesAsync();
        });

        await context.GenrePreferences.SeedIfEmptyAsync(async () =>
        {
            context.GenrePreferences.Add(new GenrePreferenceEntity { PreferenceId = 1, GenreId = 1 });
            await context.SaveChangesAsync();
        });

        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            var artists = new ArtistEntity[]
            {
                ArtistFaker.GetFaker(artistManagerIds[0], "The Rockers", "rockers.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[1], "Indie Vibes", "indievibes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[2], "Electronic Pulse", "electronicpulse.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[3], "Hip-Hop Flow", "hiphopflow.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[4], "Jazz Masters", "jazzmaster.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[5], "Always Punks", "alwayspunks.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[6], "The Hollow Frequencies", "hollowfrequencies.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[7], "Neon Foxes", "neonfoxes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[8], "Velvet Static", "velvetstatic.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[9], "Echo Bloom", "echobloom.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[10], "The Wild Chords", "wildchords.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[11], "Glitch & Glow", "glitchandglow.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[12], "Sonic Mirage", "sonicmirage.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[13], "Neon Echoes", "neonechoes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[14], "Dreamwave Collective", "dreamwavecollective.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[15], "Synth Pulse", "synthpulse.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[16], "The Brass Poets", "brasspoets.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[17], "Groove Alchemy", "groovealchemy.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[18], "Velvet Rhymes", "velvetrhymes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[19], "The Lo-Fi Syndicate", "lofisyndicate.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[20], "Beats & Blue Notes", "beatsbluenotes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[21], "Bass Pilots", "basspilots.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[22], "The Digital Prophets", "digitalprophets.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[23], "Neon Bass Theory", "neonbasstheory.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[24], "Wavelength 303", "wavelength303.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[25], "Gravity Loops", "gravityloops.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[26], "The Golden Reverie", "goldenreverie.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[27], "Fable Sound", "fablesound.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[28], "Moonlight Static", "moonlightstatic.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[29], "The Chromatics", "thechromatics.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[30], "Echo Reverberation", "echoreverberation.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[31], "Midnight Reverie", "midnightreverie.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[32], "Static Wolves", "staticwolves.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[33], "Echo Collapse", "echocollapse.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[34], "Violet Sundown", "violetsundown.jpg").Generate()
            };
            context.Artists.AddRange(artists);
            await context.SaveChangesAsync();
        });

        await context.ArtistGenres.SeedIfEmptyAsync(async () =>
        {
            var artistGenres = new ArtistGenreEntity[]
            {
                new ArtistGenreEntity { ArtistId = 1, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 1, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 1, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 2, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 2, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 2, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 3, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 3, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 4, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 5, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 5, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 6, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 6, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 7, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 8, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 8, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 9, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 9, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 10, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 10, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 11, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 11, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 12, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 13, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 13, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 14, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 15, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 16, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 17, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 18, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 19, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 20, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 21, GenreId = 8 },
                new ArtistGenreEntity { ArtistId = 22, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 23, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 24, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 25, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 26, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 27, GenreId = 8 },
                new ArtistGenreEntity { ArtistId = 28, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 29, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 30, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 31, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 32, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 33, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 34, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 35, GenreId = 8 },
            };
            context.ArtistGenres.AddRange(artistGenres);
            await context.SaveChangesAsync();
        });

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
            context.Venues.AddRange(venues);
            await context.SaveChangesAsync();
        });

        await context.Opportunities.SeedIfEmptyAsync(async () =>
        {
            var opportunities = new OpportunityEntity[]
            {
                OpportunityEntity.Create(1, new DateRange(now.AddDays(-60), now.AddDays(-60).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(2, new DateRange(now.AddDays(-55), now.AddDays(-55).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityEntity.Create(3, new DateRange(now.AddDays(-50), now.AddDays(-50).AddHours(3)), FlatFeeContractEntity.Create(180, PaymentMethod.Cash)),
                OpportunityEntity.Create(4, new DateRange(now.AddDays(-45), now.AddDays(-45).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityEntity.Create(5, new DateRange(now.AddDays(-40), now.AddDays(-40).AddHours(3)), FlatFeeContractEntity.Create(160, PaymentMethod.Cash)),
                OpportunityEntity.Create(6, new DateRange(now.AddDays(-35), now.AddDays(-35).AddHours(3)), FlatFeeContractEntity.Create(220, PaymentMethod.Cash)),
                OpportunityEntity.Create(7, new DateRange(now.AddDays(-30), now.AddDays(-30).AddHours(3)), FlatFeeContractEntity.Create(210, PaymentMethod.Cash)),
                OpportunityEntity.Create(8, new DateRange(now.AddDays(-25), now.AddDays(-25).AddHours(3)), FlatFeeContractEntity.Create(230, PaymentMethod.Cash)),
                OpportunityEntity.Create(9, new DateRange(now.AddDays(-20), now.AddDays(-20).AddHours(3)), FlatFeeContractEntity.Create(240, PaymentMethod.Cash)),
                OpportunityEntity.Create(10, new DateRange(now.AddDays(-15), now.AddDays(-15).AddHours(3)), FlatFeeContractEntity.Create(250, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(-10), now.AddDays(-10).AddHours(3)), FlatFeeContractEntity.Create(160, PaymentMethod.Cash)),
                OpportunityEntity.Create(2, new DateRange(now.AddDays(-5), now.AddDays(-5).AddHours(3)), FlatFeeContractEntity.Create(300, PaymentMethod.Cash)),
                OpportunityEntity.Create(3, new DateRange(now, now.AddHours(3)), FlatFeeContractEntity.Create(280, PaymentMethod.Cash)),
                OpportunityEntity.Create(4, new DateRange(now.AddDays(5), now.AddDays(5).AddHours(3)), FlatFeeContractEntity.Create(270, PaymentMethod.Transfer)),
                OpportunityEntity.Create(5, new DateRange(now.AddDays(10), now.AddDays(10).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash)),
                OpportunityEntity.Create(6, new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)), DoorSplitContractEntity.Create(65, PaymentMethod.Cash)),
                OpportunityEntity.Create(7, new DateRange(now.AddDays(20), now.AddDays(20).AddHours(3)), VersusContractEntity.Create(150, 70, PaymentMethod.Cash)),
                OpportunityEntity.Create(8, new DateRange(now.AddDays(25), now.AddDays(25).AddHours(3)), VersusContractEntity.Create(200, 70, PaymentMethod.Transfer)),
                OpportunityEntity.Create(9, new DateRange(now.AddDays(30), now.AddDays(30).AddHours(3)), FlatFeeContractEntity.Create(245, PaymentMethod.Transfer)),
                OpportunityEntity.Create(10, new DateRange(now.AddDays(35), now.AddDays(35).AddHours(3)), FlatFeeContractEntity.Create(240, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(40), now.AddDays(40).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Transfer)),
                OpportunityEntity.Create(2, new DateRange(now.AddDays(45), now.AddDays(45).AddHours(3)), FlatFeeContractEntity.Create(230, PaymentMethod.Cash)),
                OpportunityEntity.Create(3, new DateRange(now.AddDays(50), now.AddDays(50).AddHours(3)), FlatFeeContractEntity.Create(225, PaymentMethod.Cash)),
                OpportunityEntity.Create(4, new DateRange(now.AddDays(55), now.AddDays(55).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash)),
                OpportunityEntity.Create(5, new DateRange(now.AddDays(60), now.AddDays(60).AddHours(3)), FlatFeeContractEntity.Create(215, PaymentMethod.Cash)),
                OpportunityEntity.Create(6, new DateRange(now.AddDays(65), now.AddDays(65).AddHours(3)), FlatFeeContractEntity.Create(210, PaymentMethod.Cash)),
                OpportunityEntity.Create(7, new DateRange(now.AddDays(70), now.AddDays(70).AddHours(3)), FlatFeeContractEntity.Create(205, PaymentMethod.Cash)),
                OpportunityEntity.Create(8, new DateRange(now.AddDays(75), now.AddDays(75).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityEntity.Create(9, new DateRange(now.AddDays(80), now.AddDays(80).AddHours(3)), FlatFeeContractEntity.Create(195, PaymentMethod.Cash)),
                OpportunityEntity.Create(10, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(3)), FlatFeeContractEntity.Create(190, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(3)), FlatFeeContractEntity.Create(190, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(5)), FlatFeeContractEntity.Create(190, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(2), now.AddDays(2).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(4), now.AddDays(4).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(6), now.AddDays(6).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityEntity.Create(2, new DateRange(now.AddDays(8), now.AddDays(8).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(2, new DateRange(now.AddDays(10), now.AddDays(10).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityEntity.Create(2, new DateRange(now.AddDays(12), now.AddDays(12).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityEntity.Create(3, new DateRange(now.AddDays(14), now.AddDays(14).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(3, new DateRange(now.AddDays(16), now.AddDays(16).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityEntity.Create(3, new DateRange(now.AddDays(18), now.AddDays(18).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityEntity.Create(4, new DateRange(now.AddDays(22), now.AddDays(22).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(5, new DateRange(now.AddDays(24), now.AddDays(24).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(6, new DateRange(now.AddDays(26), now.AddDays(26).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(30), now.AddDays(30).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(32), now.AddDays(32).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(34), now.AddDays(34).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(36), now.AddDays(36).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(38), now.AddDays(38).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(60), now.AddDays(60).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(90), now.AddDays(90).AddHours(3)), VersusContractEntity.Create(100, 70, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(120), now.AddDays(120).AddHours(3)), VenueHireContractEntity.Create(250, PaymentMethod.Transfer)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(150), now.AddDays(150).AddHours(3)), DoorSplitContractEntity.Create(65, PaymentMethod.Cash)),
                OpportunityEntity.Create(1, new DateRange(now.AddDays(180), now.AddDays(180).AddHours(3)), VersusContractEntity.Create(150, 60, PaymentMethod.Cash)),
            };
            context.Opportunities.AddRange(opportunities);
            await context.SaveChangesAsync();
        });

        await context.OpportunityGenres.SeedIfEmptyAsync(async () =>
        {
            var opportunityGenres = new OpportunityGenreEntity[]
            {
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 2, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 3, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 4, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 7, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 10, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 12, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 13, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 14, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 15, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 17, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 18, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 19, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 20, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 21, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 23, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 24, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 25, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 27, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 28, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 29, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 31, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 32, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 33, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 35, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 36, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 37, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 38, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 39, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 40, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 8 }
            };
            context.OpportunityGenres.AddRange(opportunityGenres);
            await context.SaveChangesAsync();
        });

        await context.OpportunityApplications.SeedIfEmptyAsync(async () =>
        {
            var applications = new OpportunityApplicationEntity[]
            {
                OpportunityApplicationEntity.Create(1, 1),
                OpportunityApplicationEntity.Create(2, 1),
                OpportunityApplicationEntity.Create(3, 1),
                OpportunityApplicationEntity.Create(4, 1),
                OpportunityApplicationEntity.Create(1, 2),
                OpportunityApplicationEntity.Create(2, 2),
                OpportunityApplicationEntity.Create(5, 2),
                OpportunityApplicationEntity.Create(6, 2),
                OpportunityApplicationEntity.Create(1, 3),
                OpportunityApplicationEntity.Create(2, 3),
                OpportunityApplicationEntity.Create(7, 3),
                OpportunityApplicationEntity.Create(8, 3),
                OpportunityApplicationEntity.Create(1, 4),
                OpportunityApplicationEntity.Create(2, 4),
                OpportunityApplicationEntity.Create(9, 4),
                OpportunityApplicationEntity.Create(10, 4),
                OpportunityApplicationEntity.Create(1, 5),
                OpportunityApplicationEntity.Create(2, 5),
                OpportunityApplicationEntity.Create(11, 5),
                OpportunityApplicationEntity.Create(12, 5),
                OpportunityApplicationEntity.Create(1, 6),
                OpportunityApplicationEntity.Create(2, 6),
                OpportunityApplicationEntity.Create(13, 6),
                OpportunityApplicationEntity.Create(14, 6),
                OpportunityApplicationEntity.Create(1, 7),
                OpportunityApplicationEntity.Create(2, 7),
                OpportunityApplicationEntity.Create(15, 7),
                OpportunityApplicationEntity.Create(16, 7),
                OpportunityApplicationEntity.Create(1, 8),
                OpportunityApplicationEntity.Create(2, 8),
                OpportunityApplicationEntity.Create(17, 8),
                OpportunityApplicationEntity.Create(18, 8),
                OpportunityApplicationEntity.Create(17, 40),
                OpportunityApplicationEntity.Create(18, 41),
                OpportunityApplicationEntity.Create(1, 14),
                OpportunityApplicationEntity.Create(2, 14),
                OpportunityApplicationEntity.Create(3, 14),
                OpportunityApplicationEntity.Create(4, 14),
                OpportunityApplicationEntity.Create(5, 15),
                OpportunityApplicationEntity.Create(6, 15),
                OpportunityApplicationEntity.Create(7, 15),
                OpportunityApplicationEntity.Create(8, 15),
                OpportunityApplicationEntity.Create(9, 16),
                OpportunityApplicationEntity.Create(10, 16),
                OpportunityApplicationEntity.Create(11, 16),
                OpportunityApplicationEntity.Create(12, 16),
                OpportunityApplicationEntity.Create(13, 17),
                OpportunityApplicationEntity.Create(14, 17),
                OpportunityApplicationEntity.Create(15, 17),
                OpportunityApplicationEntity.Create(16, 17),
                OpportunityApplicationEntity.Create(1, 34),
                OpportunityApplicationEntity.Create(2, 34),
                OpportunityApplicationEntity.Create(19, 34),
                OpportunityApplicationEntity.Create(20, 34),
                OpportunityApplicationEntity.Create(1, 38),
                OpportunityApplicationEntity.Create(2, 38),
                OpportunityApplicationEntity.Create(12, 38),
                OpportunityApplicationEntity.Create(4, 38),
                OpportunityApplicationEntity.Create(1, 45),
                OpportunityApplicationEntity.Create(2, 46),
                OpportunityApplicationEntity.Create(3, 47),
                OpportunityApplicationEntity.Create(4, 48),
                OpportunityApplicationEntity.Create(5, 49),
                OpportunityApplicationEntity.Create(1, 50),
                OpportunityApplicationEntity.Create(2, 50),
                OpportunityApplicationEntity.Create(1, 51),
                OpportunityApplicationEntity.Create(2, 51),
                OpportunityApplicationEntity.Create(1, 52),
                OpportunityApplicationEntity.Create(2, 52),
                OpportunityApplicationEntity.Create(1, 31),
                OpportunityApplicationEntity.Create(2, 31),
                OpportunityApplicationEntity.Create(3, 31),
                OpportunityApplicationEntity.Create(1, 32),
                OpportunityApplicationEntity.Create(2, 32),
                OpportunityApplicationEntity.Create(3, 32),
            };
            context.OpportunityApplications.AddRange(applications);
            await context.SaveChangesAsync();
        });

        await context.Concerts.SeedIfEmptyAsync(async () =>
        {
            var concerts = new ConcertEntity[]
            {
                ConcertFaker.GetFaker(1, "Rockin' all Night", 15m, 120, 80, now.AddDays(-58)).Generate(),
                ConcertFaker.GetFaker(2, "Non Stop Party", 12m, 110, 70, now.AddDays(-55)).Generate(),
                ConcertFaker.GetFaker(3, "Super Mix", 18m, 130, 100, now.AddDays(-52)).Generate(),
                ConcertFaker.GetFaker(4, "Hip-Hop till you flip-flop", 10m, 100, 60, now.AddDays(-49)).Generate(),
                ConcertFaker.GetFaker(5, "Dance the night away", 25m, 140, 110, now.AddDays(-46)).Generate(),
                ConcertFaker.GetFaker(6, "Dizzy One", 20m, 150, 90, now.AddDays(-43)).Generate(),
                ConcertFaker.GetFaker(7, "Beers and Boombox", 30m, 170, 150, now.AddDays(-40)).Generate(),
                ConcertFaker.GetFaker(8, "Rockin' Tonight!", 16m, 130, 100, now.AddDays(-37)).Generate(),
                ConcertFaker.GetFaker(9, "Groovin' All Night", 14m, 115, 75, now.AddDays(-34)).Generate(),
                ConcertFaker.GetFaker(10, "Nonstop Vibes", 22m, 135, 100, now.AddDays(-31)).Generate(),
                ConcertFaker.GetFaker(11, "Electric Dreams", 13m, 125, 85, now.AddDays(-28)).Generate(),
                ConcertFaker.GetFaker(12, "Beat Drop Frenzy", 11m, 120, 90, now.AddDays(-25)).Generate(),
                ConcertFaker.GetFaker(13, "Summer Jam", 19m, 140, 110, now.AddDays(-22)).Generate(),
                ConcertFaker.GetFaker(14, "Midnight Madness", 17m, 135, 105, now.AddDays(-19)).Generate(),
                ConcertFaker.GetFaker(15, "Like a Boss", 21m, 145, 115, now.AddDays(-16)).Generate(),
                ConcertFaker.GetFaker(16, "Lights and Sound", 18m, 140, 120, now.AddDays(-13)).Generate(),
                ConcertFaker.GetFaker(17, "Rhythm Nation", 26m, 155, 130, now.AddDays(-10)).Generate(),
                ConcertFaker.GetFaker(18, "Bass Drop Party", 15m, 120, 100, now.AddDays(-7)).Generate(),
                ConcertFaker.GetFaker(19, "Chill & Thrill", 28m, 160, 145, now.AddDays(-4)).Generate(),
                ConcertFaker.GetFaker(20, "Vibin' till Night", 24m, 150, 130, now.AddDays(-1)).Generate(),
                ConcertFaker.GetFaker(21, "Ultimate Dance Party", 27m, 160, 140, now.AddDays(2)).Generate(),
                ConcertFaker.GetFaker(22, "Rock Your Soul", 23m, 130, 100, now.AddDays(5)).Generate(),
                ConcertFaker.GetFaker(23, "Danceaway", 29m, 155, 140, now.AddDays(8)).Generate(),
                ConcertFaker.GetFaker(24, "Bassline Groove Beats", 10m, 110, 70, now.AddDays(11)).Generate(),
                ConcertFaker.GetFaker(25, "Once in a Lifetime!", 15m, 125, 90, now.AddDays(14)).Generate(),
                ConcertFaker.GetFaker(26, "Jungle Fever", 30m, 180, 170, now.AddDays(17)).Generate(),
                ConcertFaker.GetFaker(35, "Boogie Nights", 20m, 100, 80, now.AddDays(6)).Generate(),
                ConcertFaker.GetFaker(39, "Boogie Wonderland", 25m, 120, 100, now.AddDays(12)).Generate(),
                ConcertFaker.GetFaker(42, "Bass in the Air", 30m, 140, 120, now.AddDays(18)).Generate(),
                ConcertFaker.GetFaker(45, "Jumpin and thumpin", 15m, 100, 80, now.AddDays(22)).Generate(),
                ConcertFaker.GetFaker(49, "Funk it up", 20m, 150, 130, now.AddDays(25)).Generate(),
                ConcertFaker.GetFaker(71, "Boogie it up!", 20m, 150, 130, now.AddDays(85)).Generate()
            };
            context.Concerts.AddRange(concerts);
            await context.SaveChangesAsync();
        });

        await context.ConcertGenres.SeedIfEmptyAsync(async () =>
        {
            var concertGenres = new ConcertGenreEntity[]
            {
                new ConcertGenreEntity { ConcertId = 1, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 1, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 2, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 2, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 3, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 3, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 4, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 6, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 6, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 7, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 8, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 8, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 9, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 9, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 10, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 11, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 12, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 13, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 14, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 15, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 16, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 17, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 17, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 18, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 18, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 19, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 19, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 20, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 21, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 21, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 22, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 23, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 24, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 25, GenreId = 8 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 29, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 29, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 8 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 7 },
            };
            context.ConcertGenres.AddRange(concertGenres);
            await context.SaveChangesAsync();
        });

        await context.Tickets.SeedIfEmptyAsync(async () =>
        {
            var tickets = new TicketEntity[]
            {
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[2], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[5], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 1, Array.Empty<byte>(), now.AddDays(-56)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[2], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[5], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 2, Array.Empty<byte>(), now.AddDays(-53)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[2], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[5], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[2], 3, Array.Empty<byte>(), now.AddDays(-50)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[2], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[5], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 4, Array.Empty<byte>(), now.AddDays(-47)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[2], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[2], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 5, Array.Empty<byte>(), now.AddDays(-44)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 6, Array.Empty<byte>(), now.AddDays(-43)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 6, Array.Empty<byte>(), now.AddDays(-43)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 8, Array.Empty<byte>(), now.AddDays(-38)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 8, Array.Empty<byte>(), now.AddDays(-38)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 8, Array.Empty<byte>(), now.AddDays(-37)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[5], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[5], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[0], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[3], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketEntity.Create(Guid.CreateVersion7(), artistManagerIds[1], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[0], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[1], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketEntity.Create(Guid.CreateVersion7(), customerIds[4], 26, Array.Empty<byte>(), now.AddDays(-2))
            };
            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();

            await context.Reviews.SeedIfEmptyAsync(async () =>
            {
                var reviews = new ReviewEntity[]
                {
                    ReviewEntity.Create(tickets[0].Id, 4, "Amazing performance!"),
                    ReviewEntity.Create(tickets[1].Id, 5, "Loved every moment!"),
                    ReviewEntity.Create(tickets[2].Id, 5, "Unforgettable night!"),
                    ReviewEntity.Create(tickets[3].Id, 4, "Great energy from the crowd."),
                    ReviewEntity.Create(tickets[4].Id, 3, "Good, but the sound was a bit off."),
                    ReviewEntity.Create(tickets[5].Id, 5, "Perfect setlist and vibes!"),
                    ReviewEntity.Create(tickets[6].Id, 4, "Would attend again!"),
                    ReviewEntity.Create(tickets[7].Id, 5, "Fantastic indie atmosphere."),
                    ReviewEntity.Create(tickets[8].Id, 4, "Loved the venue!"),
                    ReviewEntity.Create(tickets[9].Id, 4, "Solid performance."),
                    ReviewEntity.Create(tickets[10].Id, 5, "Caught my new favorite artist!"),
                    ReviewEntity.Create(tickets[11].Id, 3, "Good music, but crowded."),
                    ReviewEntity.Create(tickets[12].Id, 5, "Indie dream come true."),
                    ReviewEntity.Create(tickets[13].Id, 4, "Chill night out."),
                    ReviewEntity.Create(tickets[14].Id, 5, "Incredible stage presence!"),
                    ReviewEntity.Create(tickets[15].Id, 4, "Would love to see them again."),
                    ReviewEntity.Create(tickets[16].Id, 5, "Next-level visuals."),
                    ReviewEntity.Create(tickets[17].Id, 4, "Very unique sound."),
                    ReviewEntity.Create(tickets[18].Id, 4, "Great crowd energy."),
                    ReviewEntity.Create(tickets[19].Id, 5, "Absolute fire show."),
                    ReviewEntity.Create(tickets[20].Id, 5, "Perfect DnB experience."),
                    ReviewEntity.Create(tickets[21].Id, 4, "Smooth lyrical vibes."),
                    ReviewEntity.Create(tickets[22].Id, 5, "Top-tier show!"),
                    ReviewEntity.Create(tickets[23].Id, 4, "Nice intimate gig."),
                    ReviewEntity.Create(tickets[24].Id, 3, "A bit too loud but still fun."),
                    ReviewEntity.Create(tickets[25].Id, 4, "Well organized event."),
                    ReviewEntity.Create(tickets[26].Id, 5, "Really enjoyed it."),
                    ReviewEntity.Create(tickets[27].Id, 5, "Brought my friends, all loved it."),
                    ReviewEntity.Create(tickets[28].Id, 3, "Solid but expected more."),
                    ReviewEntity.Create(tickets[29].Id, 4, "The lighting was amazing!"),
                    ReviewEntity.Create(tickets[30].Id, 5, "Instant classic."),
                    ReviewEntity.Create(tickets[31].Id, 4, "Had a great time."),
                    ReviewEntity.Create(tickets[32].Id, 4, "Venue was packed with energy.")
                };
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            });
        });

        await context.Transactions.SeedIfEmptyAsync(async () =>
        {
            var settlementTransactions = new[]
            {
                SettlementTransactionEntity.Create(venueManagerIds[0], artistManagerIds[0], Guid.NewGuid().ToString(), 15000, TransactionStatus.Complete, 1),
                SettlementTransactionEntity.Create(venueManagerIds[1], artistManagerIds[1], Guid.NewGuid().ToString(), 20000, TransactionStatus.Complete, 2),
                SettlementTransactionEntity.Create(venueManagerIds[2], artistManagerIds[2], Guid.NewGuid().ToString(), 18000, TransactionStatus.Complete, 3),
                SettlementTransactionEntity.Create(venueManagerIds[3], artistManagerIds[3], Guid.NewGuid().ToString(), 17500, TransactionStatus.Complete, 4),
                SettlementTransactionEntity.Create(venueManagerIds[4], artistManagerIds[4], Guid.NewGuid().ToString(), 16000, TransactionStatus.Complete, 5),
            };
            settlementTransactions[0].CreatedAt = now.AddDays(-58);
            settlementTransactions[1].CreatedAt = now.AddDays(-55);
            settlementTransactions[2].CreatedAt = now.AddDays(-52);
            settlementTransactions[3].CreatedAt = now.AddDays(-49);
            settlementTransactions[4].CreatedAt = now.AddDays(-46);

            var ticketTransactions = new[]
            {
                TicketTransactionEntity.Create(customerIds[0], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(customerIds[1], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(customerIds[2], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(customerIds[3], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(customerIds[4], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(customerIds[5], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(artistManagerIds[0], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TicketTransactionEntity.Create(customerIds[1], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(customerIds[2], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(customerIds[3], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(customerIds[4], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(customerIds[5], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(artistManagerIds[0], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(artistManagerIds[1], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TicketTransactionEntity.Create(customerIds[2], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TicketTransactionEntity.Create(customerIds[3], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TicketTransactionEntity.Create(customerIds[4], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TicketTransactionEntity.Create(customerIds[5], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TicketTransactionEntity.Create(artistManagerIds[0], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TicketTransactionEntity.Create(artistManagerIds[1], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TicketTransactionEntity.Create(artistManagerIds[2], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
            };

            context.SettlementTransactions.AddRange(settlementTransactions);
            context.TicketTransactions.AddRange(ticketTransactions);
            await context.SaveChangesAsync();
        });
    }
}
