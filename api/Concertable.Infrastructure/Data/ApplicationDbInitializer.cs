using Application.Interfaces;
using Application.Interfaces.Auth;
using Application.Interfaces.Geometry;
using Concertable.Core.Entities.Contracts;
using Core.Entities;
using Core.Enums;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Data.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbInitializer
{
    private const string SeedPassword = "Password11!";

    private readonly ApplicationDbContext context;
    private readonly IPasswordHasher passwordHasher;
    private readonly TimeProvider timeProvider;
    private readonly IGeometryProvider geometryProvider;

    public ApplicationDbInitializer(ApplicationDbContext context, IPasswordHasher passwordHasher, TimeProvider timeProvider, IGeometryProvider geometryProvider)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
        this.timeProvider = timeProvider;
        this.geometryProvider = geometryProvider;
    }

    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!context.Users.Any())
        {
            var locations = LocationList.GetLocations();

            context.Users.Add(new UserEntity
            {
                Email = "admin1@test.com",
                PasswordHash = passwordHasher.Hash(SeedPassword),
                Role = Role.Admin,
                County = "Leicestershire",
                Town = "Loughborough",
                Location = geometryProvider.CreatePoint(51.0, -0.5)
            });

            context.Users.Add(new CustomerEntity
            {
                Email = "customer1@test.com",
                PasswordHash = passwordHasher.Hash(SeedPassword),
                Role = Role.Customer,
                County = locations[0].County,
                Town = locations[0].Town,
                Location = geometryProvider.CreatePoint(locations[0].Latitude, locations[0].Longitude),
                StripeId = "acct_1R71vrGWdDleGW3a"
            });

            for (int i = 2; i <= 6; i++)
            {
                var loc = locations[i % locations.Count];
                context.Users.Add(new CustomerEntity
                {
                    Email = $"customer{i}@test.com",
                    PasswordHash = passwordHasher.Hash(SeedPassword),
                    Role = Role.Customer,
                    County = loc.County,
                    Town = loc.Town,
                    Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude)
                });
            }

            context.Users.Add(new ArtistManagerEntity
            {
                Email = "artistmanager1@test.com",
                PasswordHash = passwordHasher.Hash(SeedPassword),
                Role = Role.ArtistManager,
                County = locations[0].County,
                Town = locations[0].Town,
                Location = geometryProvider.CreatePoint(locations[0].Latitude, locations[0].Longitude),
                StripeId = "acct_1R71yoLnJh1ZDYF4"
            });
            context.Users.Add(new ArtistManagerEntity
            {
                Email = "artistmanager2@test.com",
                PasswordHash = passwordHasher.Hash(SeedPassword),
                Role = Role.ArtistManager,
                County = locations[0].County,
                Town = locations[0].Town,
                Location = geometryProvider.CreatePoint(locations[0].Latitude, locations[0].Longitude),
                StripeId = "acct_1R71z6IBXwkKnqix"
            });
            for (int i = 3; i <= 35; i++)
            {
                var loc = locations[i % locations.Count];
                context.Users.Add(new ArtistManagerEntity
                {
                    Email = $"artistmanager{i}@test.com",
                    PasswordHash = passwordHasher.Hash(SeedPassword),
                    Role = Role.ArtistManager,
                    County = loc.County,
                    Town = loc.Town,
                    Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude)
                });
            }

            context.Users.Add(new VenueManagerEntity
            {
                Email = "venuemanager1@test.com",
                PasswordHash = passwordHasher.Hash(SeedPassword),
                Role = Role.VenueManager,
                County = locations[0].County,
                Town = locations[0].Town,
                Location = geometryProvider.CreatePoint(locations[0].Latitude, locations[0].Longitude),
                StripeId = "acct_1R71zKBsonWwC9oM"
            });
            context.Users.Add(new VenueManagerEntity
            {
                Email = "venuemanager2@test.com",
                PasswordHash = passwordHasher.Hash(SeedPassword),
                Role = Role.VenueManager,
                County = locations[0].County,
                Town = locations[0].Town,
                Location = geometryProvider.CreatePoint(locations[0].Latitude, locations[0].Longitude),
                StripeId = "acct_1R71zvLnLloN6AmB"
            });
            for (int i = 3; i <= 35; i++)
            {
                var loc = locations[i % locations.Count];
                context.Users.Add(new VenueManagerEntity
                {
                    Email = $"venuemanager{i}@test.com",
                    PasswordHash = passwordHasher.Hash(SeedPassword),
                    Role = Role.VenueManager,
                    County = loc.County,
                    Town = loc.Town,
                    Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude)
                });
            }

            await context.SaveChangesAsync();
        }

        // Look up user GUIDs by email for use in related entities
        var userIdsByEmail = await context.Users
            .ToDictionaryAsync(u => u.Email, u => u.Id);

        Guid GetUserId(string email) => userIdsByEmail[email];
        Guid GetCustomer(int n) => GetUserId(n == 1 ? "customer1@test.com" : $"customer{n}@test.com");
        Guid GetArtistManager(int n) => GetUserId($"artistmanager{n}@test.com");
        Guid GetVenueManager(int n) => GetUserId($"venuemanager{n}@test.com");

        //Preferences
        if (!context.Preferences.Any())
        {
            var preferences = new PreferenceEntity[]
            {
                new PreferenceEntity
                {
                    UserId = GetCustomer(1),
                    RadiusKm = 10
                }
            };
            context.Preferences.AddRange(preferences);
            await context.SaveChangesAsync();
        }

        // Genres
        if (!context.Genres.Any())
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
        }

        //PreferenceGenres
        if (context.GenrePreferences.Any())
        {
            var genrePreferences = new GenrePreferenceEntity[]
            {
                    new GenrePreferenceEntity
                    {
                        PreferenceId = 1,
                        GenreId = 1
                    }
            };
            context.GenrePreferences.AddRange(genrePreferences);
        }

        // Artists
        if (!context.Artists.Any())
        {
            var artists = new ArtistEntity[]
            {
                ArtistFaker.GetFaker(GetArtistManager(1),  "The Rockers",              "rockers.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(2),  "Indie Vibes",              "indievibes.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(3),  "Electronic Pulse",         "electronicpulse.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(4),  "Hip-Hop Flow",             "hiphopflow.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(5),  "Jazz Masters",             "jazzmaster.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(6),  "Always Punks",             "alwayspunks.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(7),  "The Hollow Frequencies",   "hollowfrequencies.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(8),  "Neon Foxes",               "neonfoxes.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(9),  "Velvet Static",            "velvetstatic.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(10), "Echo Bloom",               "echobloom.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(11), "The Wild Chords",          "wildchords.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(12), "Glitch & Glow",            "glitchandglow.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(13), "Sonic Mirage",             "sonicmirage.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(14), "Neon Echoes",              "neonechoes.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(15), "Dreamwave Collective",     "dreamwavecollective.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(16), "Synth Pulse",              "synthpulse.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(17), "The Brass Poets",          "brasspoets.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(18), "Groove Alchemy",           "groovealchemy.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(19), "Velvet Rhymes",            "velvetrhymes.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(20), "The Lo-Fi Syndicate",      "lofisyndicate.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(21), "Beats & Blue Notes",       "beatsbluenotes.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(22), "Bass Pilots",              "basspilots.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(23), "The Digital Prophets",     "digitalprophets.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(24), "Neon Bass Theory",         "neonbasstheory.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(25), "Wavelength 303",           "wavelength303.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(26), "Gravity Loops",            "gravityloops.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(27), "The Golden Reverie",       "goldenreverie.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(28), "Fable Sound",              "fablesound.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(29), "Moonlight Static",         "moonlightstatic.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(30), "The Chromatics",           "thechromatics.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(31), "Echo Reverberation",       "echoreverberation.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(32), "Midnight Reverie",         "midnightreverie.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(33), "Static Wolves",            "staticwolves.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(34), "Echo Collapse",            "echocollapse.jpg").Generate(),
                ArtistFaker.GetFaker(GetArtistManager(35), "Violet Sundown",           "violetsundown.jpg").Generate()
            };
            context.Artists.AddRange(artists);
            await context.SaveChangesAsync();
        }

        // ArtistEntity Genres
        if (!context.ArtistGenres.Any())
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
        }

        // Venues
        if (!context.Venues.Any())
        {
            var venues = new VenueEntity[]
            {
                VenueFaker.GetFaker(GetVenueManager(1),  "The Grand Venue",                  "grandvenue.jpg").Generate(), //1
                VenueFaker.GetFaker(GetVenueManager(2),  "Redhill Hall",                     "redhillhall.jpg").Generate(), //2
                VenueFaker.GetFaker(GetVenueManager(3),  "Weybridge Pavilion",               "weybridgepavilon.jpg").Generate(), //3
                VenueFaker.GetFaker(GetVenueManager(4),  "Cobham Arts Centre",               "cobhamarts.jpg").Generate(), //4
                VenueFaker.GetFaker(GetVenueManager(5),  "Chertsey Arena",                   "chertseyarena.jpg").Generate(), //5
                VenueFaker.GetFaker(GetVenueManager(6),  "Camden Electric Ballroom",         "camdenballroom.jpg").Generate(), //6
                VenueFaker.GetFaker(GetVenueManager(7),  "Manchester Night & Day Café",      "manchesternightday.jpg").Generate(), //7
                VenueFaker.GetFaker(GetVenueManager(8),  "Birmingham O2 Institute",          "birminghamo2.jpg").Generate(), //8
                VenueFaker.GetFaker(GetVenueManager(9),  "Edinburgh Usher Hall",             "edinburghusher.jpg").Generate(), //9
                VenueFaker.GetFaker(GetVenueManager(10), "Liverpool Philharmonic Hall",      "liverpoolphilharmonic.jpg").Generate(), //10
                VenueFaker.GetFaker(GetVenueManager(11), "Leeds Brudenell Social Club",      "leedsbrudenell.jpg").Generate(), //11
                VenueFaker.GetFaker(GetVenueManager(12), "Glasgow Barrowland Ballroom",      "glasgowbarrowland.jpg").Generate(), //12
                VenueFaker.GetFaker(GetVenueManager(13), "Sheffield Leadmill",               "sheffieldleadmill.jpg").Generate(), //13
                VenueFaker.GetFaker(GetVenueManager(14), "Nottingham Rock City",             "nottinghamrockcity.jpg").Generate(), //14
                VenueFaker.GetFaker(GetVenueManager(15), "Bristol Thekla",                  "bristolthekla.jpg").Generate(), //15
                VenueFaker.GetFaker(GetVenueManager(16), "Brighton Concorde 2",             "brightonconcorde2.jpg").Generate(), //16
                VenueFaker.GetFaker(GetVenueManager(17), "Cardiff Tramshed",                "cardifftramshed.jpg").Generate(), //17
                VenueFaker.GetFaker(GetVenueManager(18), "Newcastle O2 Academy",            "newcastleo2.jpg").Generate(), //18
                VenueFaker.GetFaker(GetVenueManager(19), "Oxford O2 Academy",               "oxfordo2.jpg").Generate(), //19
                VenueFaker.GetFaker(GetVenueManager(20), "Cambridge Corn Exchange",         "cambridgecornexchange.jpg").Generate(), //20
                VenueFaker.GetFaker(GetVenueManager(21), "Bath Komedia",                    "bathkomedia.jpg").Generate(), //21
                VenueFaker.GetFaker(GetVenueManager(22), "Aberdeen The Lemon Tree",         "aberdeenlemontree.jpg").Generate(), //22
                VenueFaker.GetFaker(GetVenueManager(23), "York Barbican",                   "yorkbarbican.jpg").Generate(), //23
                VenueFaker.GetFaker(GetVenueManager(24), "Belfast Limelight",               "belfastlimelight.jpg").Generate(), //24
                VenueFaker.GetFaker(GetVenueManager(25), "Dublin Vicar Street",             "dublinvicarstreet.jpg").Generate(), //25
                VenueFaker.GetFaker(GetVenueManager(26), "Norwich Waterfront",              "norwichwaterfront.jpg").Generate(), //26
                VenueFaker.GetFaker(GetVenueManager(27), "Exeter Phoenix",                  "exeterphoenix.jpg").Generate(), //27
                VenueFaker.GetFaker(GetVenueManager(28), "Southampton Engine Rooms",        "southamptonengine.jpg").Generate(), //28
                VenueFaker.GetFaker(GetVenueManager(29), "Hull The Welly Club",             "hullwellyclub.jpg").Generate(), //29
                VenueFaker.GetFaker(GetVenueManager(30), "Plymouth Junction",               "plymouthjunction.jpg").Generate(), //30
                VenueFaker.GetFaker(GetVenueManager(31), "Swansea Sin City",                "swanseasincity.jpg").Generate(), //31
                VenueFaker.GetFaker(GetVenueManager(32), "Inverness Ironworks",             "invernessironworks.jpg").Generate(), //32
                VenueFaker.GetFaker(GetVenueManager(33), "Stirling Albert Halls",           "stirlingalberthalls.jpg").Generate(), //33
                VenueFaker.GetFaker(GetVenueManager(34), "Dundee Fat Sams",                 "dundeefatsams.jpg").Generate(), //34
                VenueFaker.GetFaker(GetVenueManager(35), "Coventry Empire",                 "coventryempire.jpg").Generate()  //35
            };
            context.Venues.AddRange(venues);
            await context.SaveChangesAsync();
        }

        // ConcertEntity Opportunities
        if (!context.ConcertOpportunities.Any())
        {
            var opportunities = new ConcertOpportunityEntity[]
            {
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(-60), EndDate = now.AddDays(-60).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 150, PaymentMethod = PaymentMethod.Cash } }, //1
            new ConcertOpportunityEntity { VenueId = 2, StartDate = now.AddDays(-55), EndDate = now.AddDays(-55).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 200, PaymentMethod = PaymentMethod.Cash } }, //2
            new ConcertOpportunityEntity { VenueId = 3, StartDate = now.AddDays(-50), EndDate = now.AddDays(-50).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 180, PaymentMethod = PaymentMethod.Cash } }, //3
            new ConcertOpportunityEntity { VenueId = 4, StartDate = now.AddDays(-45), EndDate = now.AddDays(-45).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 175, PaymentMethod = PaymentMethod.Cash } }, //4
            new ConcertOpportunityEntity { VenueId = 5, StartDate = now.AddDays(-40), EndDate = now.AddDays(-40).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 160, PaymentMethod = PaymentMethod.Cash } }, //5
            new ConcertOpportunityEntity { VenueId = 6, StartDate = now.AddDays(-35), EndDate = now.AddDays(-35).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 220, PaymentMethod = PaymentMethod.Cash } }, //6
            new ConcertOpportunityEntity { VenueId = 7, StartDate = now.AddDays(-30), EndDate = now.AddDays(-30).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 210, PaymentMethod = PaymentMethod.Cash } }, //7
            new ConcertOpportunityEntity { VenueId = 8, StartDate = now.AddDays(-25), EndDate = now.AddDays(-25).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 230, PaymentMethod = PaymentMethod.Cash } }, //8
            new ConcertOpportunityEntity { VenueId = 9, StartDate = now.AddDays(-20), EndDate = now.AddDays(-20).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 240, PaymentMethod = PaymentMethod.Cash } }, //9
            new ConcertOpportunityEntity { VenueId = 10, StartDate = now.AddDays(-15), EndDate = now.AddDays(-15).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 250, PaymentMethod = PaymentMethod.Cash } }, //10
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(-10), EndDate = now.AddDays(-10).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 160, PaymentMethod = PaymentMethod.Cash } }, //11
            new ConcertOpportunityEntity { VenueId = 2, StartDate = now.AddDays(-5), EndDate = now.AddDays(-5).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 300, PaymentMethod = PaymentMethod.Cash } }, //12
            new ConcertOpportunityEntity { VenueId = 3, StartDate = now, EndDate = now.AddHours(3), Contract = new FlatFeeContractEntity { Fee = 280, PaymentMethod = PaymentMethod.Cash } }, //13
            new ConcertOpportunityEntity { VenueId = 4, StartDate = now.AddDays(5), EndDate = now.AddDays(5).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 270, PaymentMethod = PaymentMethod.Transfer } }, //14
            new ConcertOpportunityEntity { VenueId = 5, StartDate = now.AddDays(10), EndDate = now.AddDays(10).AddHours(3), Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70, PaymentMethod = PaymentMethod.Cash } }, //15
            new ConcertOpportunityEntity { VenueId = 6, StartDate = now.AddDays(15), EndDate = now.AddDays(15).AddHours(3), Contract = new DoorSplitContractEntity { ArtistDoorPercent = 65, PaymentMethod = PaymentMethod.Cash } }, //16
            new ConcertOpportunityEntity { VenueId = 7, StartDate = now.AddDays(20), EndDate = now.AddDays(20).AddHours(3), Contract = new VersusContractEntity { Guarantee = 150, ArtistDoorPercent = 70, PaymentMethod = PaymentMethod.Cash } }, //17
            new ConcertOpportunityEntity { VenueId = 8, StartDate = now.AddDays(25), EndDate = now.AddDays(25).AddHours(3), Contract = new VersusContractEntity { Guarantee = 200, ArtistDoorPercent = 70, PaymentMethod = PaymentMethod.Transfer } }, //18
            new ConcertOpportunityEntity { VenueId = 9, StartDate = now.AddDays(30), EndDate = now.AddDays(30).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 245, PaymentMethod = PaymentMethod.Transfer } }, //19
            new ConcertOpportunityEntity { VenueId = 10, StartDate = now.AddDays(35), EndDate = now.AddDays(35).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 240, PaymentMethod = PaymentMethod.Cash } }, //20
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(40), EndDate = now.AddDays(40).AddHours(3), Contract = new VenueHireContractEntity { HireFee = 300, PaymentMethod = PaymentMethod.Transfer } }, //21
            new ConcertOpportunityEntity { VenueId = 2, StartDate = now.AddDays(45), EndDate = now.AddDays(45).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 230, PaymentMethod = PaymentMethod.Cash } }, //22
            new ConcertOpportunityEntity { VenueId = 3, StartDate = now.AddDays(50), EndDate = now.AddDays(50).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 225, PaymentMethod = PaymentMethod.Cash } }, //23
            new ConcertOpportunityEntity { VenueId = 4, StartDate = now.AddDays(55), EndDate = now.AddDays(55).AddHours(3), Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70, PaymentMethod = PaymentMethod.Cash } }, //24
            new ConcertOpportunityEntity { VenueId = 5, StartDate = now.AddDays(60), EndDate = now.AddDays(60).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 215, PaymentMethod = PaymentMethod.Cash } }, //25
            new ConcertOpportunityEntity { VenueId = 6, StartDate = now.AddDays(65), EndDate = now.AddDays(65).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 210, PaymentMethod = PaymentMethod.Cash } }, //26
            new ConcertOpportunityEntity { VenueId = 7, StartDate = now.AddDays(70), EndDate = now.AddDays(70).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 205, PaymentMethod = PaymentMethod.Cash } }, //27
            new ConcertOpportunityEntity { VenueId = 8, StartDate = now.AddDays(75), EndDate = now.AddDays(75).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 200, PaymentMethod = PaymentMethod.Cash } }, //28
            new ConcertOpportunityEntity { VenueId = 9, StartDate = now.AddDays(80), EndDate = now.AddDays(80).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 195, PaymentMethod = PaymentMethod.Cash } }, //29
            new ConcertOpportunityEntity { VenueId = 10, StartDate = now.AddDays(85), EndDate = now.AddDays(85).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 190, PaymentMethod = PaymentMethod.Cash } }, //30
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(85), EndDate = now.AddDays(85).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 190, PaymentMethod = PaymentMethod.Cash } }, //31
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(85), EndDate = now.AddDays(85).AddHours(5), Contract = new FlatFeeContractEntity { Fee = 190, PaymentMethod = PaymentMethod.Cash } }, //32
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(2), EndDate = now.AddDays(2).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 150, PaymentMethod = PaymentMethod.Cash } }, //33
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(4), EndDate = now.AddDays(4).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 175, PaymentMethod = PaymentMethod.Cash } }, //34
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(6), EndDate = now.AddDays(6).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 200, PaymentMethod = PaymentMethod.Cash } }, //35
            new ConcertOpportunityEntity { VenueId = 2, StartDate = now.AddDays(8), EndDate = now.AddDays(8).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 150, PaymentMethod = PaymentMethod.Cash } }, //36
            new ConcertOpportunityEntity { VenueId = 2, StartDate = now.AddDays(10), EndDate = now.AddDays(10).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 175, PaymentMethod = PaymentMethod.Cash } }, //37
            new ConcertOpportunityEntity { VenueId = 2, StartDate = now.AddDays(12), EndDate = now.AddDays(12).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 200, PaymentMethod = PaymentMethod.Cash } }, //38
            new ConcertOpportunityEntity { VenueId = 3, StartDate = now.AddDays(14), EndDate = now.AddDays(14).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 150, PaymentMethod = PaymentMethod.Cash } }, //39
            new ConcertOpportunityEntity { VenueId = 3, StartDate = now.AddDays(16), EndDate = now.AddDays(16).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 175, PaymentMethod = PaymentMethod.Cash } }, //40
            new ConcertOpportunityEntity { VenueId = 3, StartDate = now.AddDays(18), EndDate = now.AddDays(18).AddHours(3), Contract = new FlatFeeContractEntity { Fee = 200, PaymentMethod = PaymentMethod.Cash } }, //41
            new ConcertOpportunityEntity { VenueId = 4, StartDate = now.AddDays(22), EndDate = now.AddDays(22).AddHours(3) }, //42 - no contract
            new ConcertOpportunityEntity { VenueId = 5, StartDate = now.AddDays(24), EndDate = now.AddDays(24).AddHours(3) }, //43 - no contract
            new ConcertOpportunityEntity { VenueId = 6, StartDate = now.AddDays(26), EndDate = now.AddDays(26).AddHours(3) }, //44 - no contract

            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(30),  EndDate = now.AddDays(30).AddHours(3),  Contract = new FlatFeeContractEntity   { Fee = 200, PaymentMethod = PaymentMethod.Transfer } }, //45
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(32),  EndDate = now.AddDays(32).AddHours(3),  Contract = new FlatFeeContractEntity   { Fee = 200, PaymentMethod = PaymentMethod.Transfer } }, //46
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(34),  EndDate = now.AddDays(34).AddHours(3),  Contract = new FlatFeeContractEntity   { Fee = 200, PaymentMethod = PaymentMethod.Transfer } }, //47
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(36),  EndDate = now.AddDays(36).AddHours(3),  Contract = new FlatFeeContractEntity   { Fee = 200, PaymentMethod = PaymentMethod.Transfer } }, //48
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(38),  EndDate = now.AddDays(38).AddHours(3),  Contract = new FlatFeeContractEntity   { Fee = 200, PaymentMethod = PaymentMethod.Transfer } }, //49
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(60),  EndDate = now.AddDays(60).AddHours(3),  Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70, PaymentMethod = PaymentMethod.Transfer } }, //50
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(90),  EndDate = now.AddDays(90).AddHours(3),  Contract = new VersusContractEntity    { Guarantee = 100, ArtistDoorPercent = 70, PaymentMethod = PaymentMethod.Transfer } }, //51
            new ConcertOpportunityEntity { VenueId = 1, StartDate = now.AddDays(120), EndDate = now.AddDays(120).AddHours(3), Contract = new VenueHireContractEntity { HireFee = 250, PaymentMethod = PaymentMethod.Transfer } }, //52

            };
            context.ConcertOpportunities.AddRange(opportunities);
            await context.SaveChangesAsync();
        }

        // OpportunityGenres
        if (!context.OpportunityGenres.Any())
        {
            var opportunityGenres = new OpportunityGenreEntity[]
            {
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 2, GenreId = 5 }, // Electronic
                new OpportunityGenreEntity { OpportunityId = 3, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 4, GenreId = 4 }, // Hip-Hop
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 4 }, // Hip-Hop
                new OpportunityGenreEntity { OpportunityId = 7, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 4 }, // Hip-Hop
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 10, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 5 }, // Electronic
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 12, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 13, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 14, GenreId = 7 }, // DnB
                new OpportunityGenreEntity { OpportunityId = 15, GenreId = 8 }, // House
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 7 }, // DnB
                new OpportunityGenreEntity { OpportunityId = 17, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 18, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 19, GenreId = 4 }, // Hip-Hop
                new OpportunityGenreEntity { OpportunityId = 20, GenreId = 7 }, // DnB
                new OpportunityGenreEntity { OpportunityId = 21, GenreId = 8 }, // House
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 23, GenreId = 5 }, // Electronic
                new OpportunityGenreEntity { OpportunityId = 24, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 25, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 5 }, // Electronic
                new OpportunityGenreEntity { OpportunityId = 27, GenreId = 8 }, // House
                new OpportunityGenreEntity { OpportunityId = 28, GenreId = 5 }, // Electronic
                new OpportunityGenreEntity { OpportunityId = 29, GenreId = 7 }, // DnB
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 31, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 32, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 33, GenreId = 4 }, // Hip-Hop
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 35, GenreId = 8 }, // House
                new OpportunityGenreEntity { OpportunityId = 36, GenreId = 6 }, // Indie
                new OpportunityGenreEntity { OpportunityId = 37, GenreId = 7 }, // DnB
                new OpportunityGenreEntity { OpportunityId = 38, GenreId = 3 }, // Jazz
                new OpportunityGenreEntity { OpportunityId = 39, GenreId = 1 }, // Rock
                new OpportunityGenreEntity { OpportunityId = 40, GenreId = 2 }, // Pop
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 4 }, // Hip-Hop
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 8 }  // House
            };
            context.OpportunityGenres.AddRange(opportunityGenres);
            await context.SaveChangesAsync();
        }

        // ConcertEntity Applications
        if (!context.ConcertApplications.Any())
        {
            var applications = new ConcertApplicationEntity[]
            {
                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 1 }, //1
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 1 }, //2
                new ConcertApplicationEntity { ArtistId = 3, OpportunityId = 1 }, //3
                new ConcertApplicationEntity { ArtistId = 4, OpportunityId = 1 }, //4

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 2 }, //5
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 2 }, //6
                new ConcertApplicationEntity { ArtistId = 5, OpportunityId = 2 }, //7
                new ConcertApplicationEntity { ArtistId = 6, OpportunityId = 2 }, //8

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 3 }, //9
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 3 }, //10
                new ConcertApplicationEntity { ArtistId = 7, OpportunityId = 3 }, //11
                new ConcertApplicationEntity { ArtistId = 8, OpportunityId = 3 }, //12

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 4 }, //13
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 4 }, //14
                new ConcertApplicationEntity { ArtistId = 9, OpportunityId = 4 }, //15
                new ConcertApplicationEntity { ArtistId = 10, OpportunityId = 4 }, //16

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 5 }, //17
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 5 }, //18
                new ConcertApplicationEntity { ArtistId = 11, OpportunityId = 5 }, //19
                new ConcertApplicationEntity { ArtistId = 12, OpportunityId = 5 }, //20

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 6 }, //21
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 6 }, //22
                new ConcertApplicationEntity { ArtistId = 13, OpportunityId = 6 }, //23
                new ConcertApplicationEntity { ArtistId = 14, OpportunityId = 6 }, //24

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 7 }, //25
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 7 }, //26
                new ConcertApplicationEntity { ArtistId = 15, OpportunityId = 7 }, //27
                new ConcertApplicationEntity { ArtistId = 16, OpportunityId = 7 }, //28

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 8 }, //29
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 8 }, //30
                new ConcertApplicationEntity { ArtistId = 17, OpportunityId = 8 }, //31
                new ConcertApplicationEntity { ArtistId = 18, OpportunityId = 8 }, //32
                new ConcertApplicationEntity { ArtistId = 17, OpportunityId = 40 }, //33
                new ConcertApplicationEntity { ArtistId = 18, OpportunityId = 41 }, //34

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 14 }, //35
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 14 }, //36
                new ConcertApplicationEntity { ArtistId = 3, OpportunityId = 14 }, //37
                new ConcertApplicationEntity { ArtistId = 4, OpportunityId = 14 }, //38

                new ConcertApplicationEntity { ArtistId = 5, OpportunityId = 15 }, //39
                new ConcertApplicationEntity { ArtistId = 6, OpportunityId = 15 }, //40
                new ConcertApplicationEntity { ArtistId = 7, OpportunityId = 15 }, //41
                new ConcertApplicationEntity { ArtistId = 8, OpportunityId = 15 }, //42

                new ConcertApplicationEntity { ArtistId = 9, OpportunityId = 16 }, //43
                new ConcertApplicationEntity { ArtistId = 10, OpportunityId = 16 }, //44
                new ConcertApplicationEntity { ArtistId = 11, OpportunityId = 16 }, //45
                new ConcertApplicationEntity { ArtistId = 12, OpportunityId = 16 }, //46

                new ConcertApplicationEntity { ArtistId = 13, OpportunityId = 17 }, //47
                new ConcertApplicationEntity { ArtistId = 14, OpportunityId = 17 }, //48
                new ConcertApplicationEntity { ArtistId = 15, OpportunityId = 17 }, //49
                new ConcertApplicationEntity { ArtistId = 16, OpportunityId = 17 }, //50

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 34 }, //51
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 34 }, //52
                new ConcertApplicationEntity { ArtistId = 19, OpportunityId = 34 }, //53
                new ConcertApplicationEntity { ArtistId = 20, OpportunityId = 34 }, //54

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 38 }, //55
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 38 }, //56
                new ConcertApplicationEntity { ArtistId = 12, OpportunityId = 38 }, //57
                new ConcertApplicationEntity { ArtistId = 4, OpportunityId = 38 }, //58
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 33, Status = ApplicationStatus.Confirmed }, //59 - Confirmed, for settle/complete testing

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 45 }, //60 - FlatFee test
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 46 }, //61 - FlatFee test
                new ConcertApplicationEntity { ArtistId = 3, OpportunityId = 47 }, //62 - FlatFee test
                new ConcertApplicationEntity { ArtistId = 4, OpportunityId = 48 }, //63 - FlatFee test
                new ConcertApplicationEntity { ArtistId = 5, OpportunityId = 49 }, //64 - FlatFee test
                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 50 }, //65 - DoorSplit test
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 50 }, //66 - DoorSplit test
                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 51 }, //67 - Versus test
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 51 }, //68 - Versus test
                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 52 }, //69 - VenueHire test
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 52 }, //70 - VenueHire test

                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 31 }, //71
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 31 }, //72
                new ConcertApplicationEntity { ArtistId = 3, OpportunityId = 31 }, //73
                new ConcertApplicationEntity { ArtistId = 1, OpportunityId = 32 }, //74
                new ConcertApplicationEntity { ArtistId = 2, OpportunityId = 32 }, //75
                new ConcertApplicationEntity { ArtistId = 3, OpportunityId = 32 }, //76
            };
            context.ConcertApplications.AddRange(applications);
            await context.SaveChangesAsync();
        }

        if (!context.Concerts.Any())
        {
            var concerts = new ConcertEntity[]
            {
                    ConcertFaker.GetFaker(1, "Rockin' all Night", 15m, 120, 80, now.AddDays(-58)).Generate(), //1
                    ConcertFaker.GetFaker(2, "Non Stop Party", 12m, 110, 70, now.AddDays(-55)).Generate(), //2
                    ConcertFaker.GetFaker(3, "Super Mix", 18m, 130, 100, now.AddDays(-52)).Generate(), //3
                    ConcertFaker.GetFaker(4, "Hip-Hop till you flip-flop", 10m, 100, 60, now.AddDays(-49)).Generate(), //4
                    ConcertFaker.GetFaker(5, "Dance the night away", 25m, 140, 110, now.AddDays(-46)).Generate(), //5
                    ConcertFaker.GetFaker(6, "Dizzy One", 20m, 150, 90, now.AddDays(-43)).Generate(), //6
                    ConcertFaker.GetFaker(7, "Beers and Boombox", 30m, 170, 150, now.AddDays(-40)).Generate(), //7
                    ConcertFaker.GetFaker(8, "Rockin' Tonight!", 16m, 130, 100, now.AddDays(-37)).Generate(), //8
                    ConcertFaker.GetFaker(9, "Groovin' All Night", 14m, 115, 75, now.AddDays(-34)).Generate(), //9
                    ConcertFaker.GetFaker(10, "Nonstop Vibes", 22m, 135, 100, now.AddDays(-31)).Generate(), //10
                    ConcertFaker.GetFaker(11, "Electric Dreams", 13m, 125, 85, now.AddDays(-28)).Generate(), //11
                    ConcertFaker.GetFaker(12, "Beat Drop Frenzy", 11m, 120, 90, now.AddDays(-25)).Generate(), //12
                    ConcertFaker.GetFaker(13, "Summer Jam", 19m, 140, 110, now.AddDays(-22)).Generate(), //13
                    ConcertFaker.GetFaker(14, "Midnight Madness", 17m, 135, 105, now.AddDays(-19)).Generate(), //14
                    ConcertFaker.GetFaker(15, "Like a Boss", 21m, 145, 115, now.AddDays(-16)).Generate(), //15
                    ConcertFaker.GetFaker(16, "Lights and Sound", 18m, 140, 120, now.AddDays(-13)).Generate(), //16
                    ConcertFaker.GetFaker(17, "Rhythm Nation", 26m, 155, 130, now.AddDays(-10)).Generate(), //17
                    ConcertFaker.GetFaker(18, "Bass Drop Party", 15m, 120, 100, now.AddDays(-7)).Generate(), //18
                    ConcertFaker.GetFaker(19, "Chill & Thrill", 28m, 160, 145, now.AddDays(-4)).Generate(), //19
                    ConcertFaker.GetFaker(20, "Vibin' till Night", 24m, 150, 130, now.AddDays(-1)).Generate(), //20
                    ConcertFaker.GetFaker(21, "Ultimate Dance Party", 27m, 160, 140, now.AddDays(2)).Generate(), //21
                    ConcertFaker.GetFaker(22, "Rock Your Soul", 23m, 130, 100, now.AddDays(5)).Generate(), //22
                    ConcertFaker.GetFaker(23, "Danceaway", 29m, 155, 140, now.AddDays(8)).Generate(), //23
                    ConcertFaker.GetFaker(24, "Bassline Groove Beats", 10m, 110, 70, now.AddDays(11)).Generate(), //24
                    ConcertFaker.GetFaker(25, "Once in a Lifetime!", 15m, 125, 90, now.AddDays(14)).Generate(), //25
                    ConcertFaker.GetFaker(26, "Jungle Fever", 30m, 180, 170, now.AddDays(17)).Generate(), //26
                    ConcertFaker.GetFaker(35, "Boogie Nights", 20m, 100, 80, now.AddDays(6)).Generate(), //27
                    ConcertFaker.GetFaker(39, "Boogie Wonderland", 25m, 120, 100, now.AddDays(12)).Generate(), //28
                    ConcertFaker.GetFaker(42, "Bass in the Air", 30m, 140, 120, now.AddDays(18)).Generate(), //29
                    ConcertFaker.GetFaker(45, "Jumpin and thumpin", 15m, 100, 80, now.AddDays(22)).Generate(), //30
                    ConcertFaker.GetFaker(49, "Funk it up", 20m, 150, 130, now.AddDays(25)).Generate(), //31
                    ConcertFaker.GetFaker(54, "Boogie it up!", 20m, 150, 130, now.AddDays(25)).Generate(), //32
                    ConcertFaker.GetFaker(59, "Dev FlatFee Test Concert", 10m, 50, 50, now.AddDays(-1)).Generate() //33
            };

            context.Concerts.AddRange(concerts);
            await context.SaveChangesAsync();
        }

        // ConcertGenres
        if (!context.ConcertGenres.Any())
        {
            var concertGenres = new List<ConcertGenreEntity>
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
        }


        // Tickets
        if (!context.Tickets.Any())
        {
            var c1 = GetCustomer(1);
            var c2 = GetCustomer(2);
            var c3 = GetCustomer(3);
            var c4 = GetCustomer(4);
            var c5 = GetCustomer(5);
            var c6 = GetCustomer(6);

            // artistmanager users 8-10 are used for ticket purchases in the original data
            // those mapped to customers in original seed — map to artist managers for compatibility
            var am1 = GetArtistManager(1);
            var am2 = GetArtistManager(2);
            var am3 = GetArtistManager(3);

            var tickets = new TicketEntity[]
            {
            new TicketEntity { UserId = c1, ConcertId = 1, PurchaseDate = now.AddDays(-58) },
            new TicketEntity { UserId = c2, ConcertId = 1, PurchaseDate = now.AddDays(-58) },
            new TicketEntity { UserId = c3, ConcertId = 1, PurchaseDate = now.AddDays(-58) },
            new TicketEntity { UserId = c4, ConcertId = 1, PurchaseDate = now.AddDays(-57) },
            new TicketEntity { UserId = c5, ConcertId = 1, PurchaseDate = now.AddDays(-57) },
            new TicketEntity { UserId = c6, ConcertId = 1, PurchaseDate = now.AddDays(-57) },
            new TicketEntity { UserId = am1, ConcertId = 1, PurchaseDate = now.AddDays(-56) },

            new TicketEntity { UserId = c2, ConcertId = 2, PurchaseDate = now.AddDays(-55) },
            new TicketEntity { UserId = c3, ConcertId = 2, PurchaseDate = now.AddDays(-55) },
            new TicketEntity { UserId = c4, ConcertId = 2, PurchaseDate = now.AddDays(-55) },
            new TicketEntity { UserId = c5, ConcertId = 2, PurchaseDate = now.AddDays(-54) },
            new TicketEntity { UserId = c6, ConcertId = 2, PurchaseDate = now.AddDays(-54) },
            new TicketEntity { UserId = am1, ConcertId = 2, PurchaseDate = now.AddDays(-54) },
            new TicketEntity { UserId = am2, ConcertId = 2, PurchaseDate = now.AddDays(-53) },

            new TicketEntity { UserId = c3, ConcertId = 3, PurchaseDate = now.AddDays(-52) },
            new TicketEntity { UserId = c4, ConcertId = 3, PurchaseDate = now.AddDays(-52) },
            new TicketEntity { UserId = c5, ConcertId = 3, PurchaseDate = now.AddDays(-52) },
            new TicketEntity { UserId = c6, ConcertId = 3, PurchaseDate = now.AddDays(-51) },
            new TicketEntity { UserId = am1, ConcertId = 3, PurchaseDate = now.AddDays(-51) },
            new TicketEntity { UserId = am2, ConcertId = 3, PurchaseDate = now.AddDays(-51) },
            new TicketEntity { UserId = am3, ConcertId = 3, PurchaseDate = now.AddDays(-50) },

            new TicketEntity { UserId = c1, ConcertId = 4, PurchaseDate = now.AddDays(-49) },
            new TicketEntity { UserId = c2, ConcertId = 4, PurchaseDate = now.AddDays(-49) },
            new TicketEntity { UserId = c3, ConcertId = 4, PurchaseDate = now.AddDays(-49) },
            new TicketEntity { UserId = c4, ConcertId = 4, PurchaseDate = now.AddDays(-48) },
            new TicketEntity { UserId = c5, ConcertId = 4, PurchaseDate = now.AddDays(-48) },
            new TicketEntity { UserId = c6, ConcertId = 4, PurchaseDate = now.AddDays(-48) },
            new TicketEntity { UserId = am1, ConcertId = 4, PurchaseDate = now.AddDays(-47) },

            new TicketEntity { UserId = am2, ConcertId = 5, PurchaseDate = now.AddDays(-46) },
            new TicketEntity { UserId = am3, ConcertId = 5, PurchaseDate = now.AddDays(-46) },
            new TicketEntity { UserId = c1, ConcertId = 5, PurchaseDate = now.AddDays(-46) },
            new TicketEntity { UserId = c2, ConcertId = 5, PurchaseDate = now.AddDays(-45) },
            new TicketEntity { UserId = c3, ConcertId = 5, PurchaseDate = now.AddDays(-45) },
            new TicketEntity { UserId = c4, ConcertId = 5, PurchaseDate = now.AddDays(-45) },
            new TicketEntity { UserId = c5, ConcertId = 5, PurchaseDate = now.AddDays(-44) },

            new TicketEntity { UserId = c1, ConcertId = 6, PurchaseDate = now.AddDays(-43) },
            new TicketEntity { UserId = c2, ConcertId = 6, PurchaseDate = now.AddDays(-43) },
            new TicketEntity { UserId = c4, ConcertId = 6, PurchaseDate = now.AddDays(-42) },
            new TicketEntity { UserId = c5, ConcertId = 6, PurchaseDate = now.AddDays(-42) },
            new TicketEntity { UserId = am1, ConcertId = 6, PurchaseDate = now.AddDays(-42) },

            new TicketEntity { UserId = c1, ConcertId = 7, PurchaseDate = now.AddDays(-40) },
            new TicketEntity { UserId = c2, ConcertId = 7, PurchaseDate = now.AddDays(-40) },
            new TicketEntity { UserId = am2, ConcertId = 7, PurchaseDate = now.AddDays(-40) },

            new TicketEntity { UserId = c1, ConcertId = 8, PurchaseDate = now.AddDays(-38) },
            new TicketEntity { UserId = c2, ConcertId = 8, PurchaseDate = now.AddDays(-38) },
            new TicketEntity { UserId = c5, ConcertId = 8, PurchaseDate = now.AddDays(-37) },

            new TicketEntity { UserId = c1, ConcertId = 9, PurchaseDate = now.AddDays(-36) },
            new TicketEntity { UserId = c2, ConcertId = 9, PurchaseDate = now.AddDays(-36) },
            new TicketEntity { UserId = am1, ConcertId = 9, PurchaseDate = now.AddDays(-36) },

            new TicketEntity { UserId = c1, ConcertId = 10, PurchaseDate = now.AddDays(-34) },
            new TicketEntity { UserId = c2, ConcertId = 10, PurchaseDate = now.AddDays(-34) },
            new TicketEntity { UserId = am2, ConcertId = 10, PurchaseDate = now.AddDays(-34) },

            new TicketEntity { UserId = c1, ConcertId = 11, PurchaseDate = now.AddDays(-32) },
            new TicketEntity { UserId = c2, ConcertId = 11, PurchaseDate = now.AddDays(-32) },
            new TicketEntity { UserId = c5, ConcertId = 11, PurchaseDate = now.AddDays(-32) },

            new TicketEntity { UserId = c1, ConcertId = 12, PurchaseDate = now.AddDays(-30) },
            new TicketEntity { UserId = c2, ConcertId = 12, PurchaseDate = now.AddDays(-30) },
            new TicketEntity { UserId = c6, ConcertId = 12, PurchaseDate = now.AddDays(-30) },

            new TicketEntity { UserId = c1, ConcertId = 13, PurchaseDate = now.AddDays(-28) },
            new TicketEntity { UserId = c2, ConcertId = 13, PurchaseDate = now.AddDays(-28) },
            new TicketEntity { UserId = am1, ConcertId = 13, PurchaseDate = now.AddDays(-28) },

            new TicketEntity { UserId = c1, ConcertId = 14, PurchaseDate = now.AddDays(-26) },
            new TicketEntity { UserId = c2, ConcertId = 14, PurchaseDate = now.AddDays(-26) },
            new TicketEntity { UserId = c5, ConcertId = 14, PurchaseDate = now.AddDays(-26) },

            new TicketEntity { UserId = c1, ConcertId = 15, PurchaseDate = now.AddDays(-24) },
            new TicketEntity { UserId = c2, ConcertId = 15, PurchaseDate = now.AddDays(-24) },
            new TicketEntity { UserId = c4, ConcertId = 15, PurchaseDate = now.AddDays(-24) },

            new TicketEntity { UserId = c1, ConcertId = 16, PurchaseDate = now.AddDays(-22) },
            new TicketEntity { UserId = c2, ConcertId = 16, PurchaseDate = now.AddDays(-22) },
            new TicketEntity { UserId = am2, ConcertId = 16, PurchaseDate = now.AddDays(-22) },

            new TicketEntity { UserId = c1, ConcertId = 17, PurchaseDate = now.AddDays(-20) },
            new TicketEntity { UserId = c2, ConcertId = 17, PurchaseDate = now.AddDays(-20) },
            new TicketEntity { UserId = c6, ConcertId = 17, PurchaseDate = now.AddDays(-20) },

            new TicketEntity { UserId = c1, ConcertId = 18, PurchaseDate = now.AddDays(-18) },
            new TicketEntity { UserId = c2, ConcertId = 18, PurchaseDate = now.AddDays(-18) },
            new TicketEntity { UserId = am1, ConcertId = 18, PurchaseDate = now.AddDays(-18) },

            new TicketEntity { UserId = c1, ConcertId = 19, PurchaseDate = now.AddDays(-16) },
            new TicketEntity { UserId = c2, ConcertId = 19, PurchaseDate = now.AddDays(-16) },
            new TicketEntity { UserId = c5, ConcertId = 19, PurchaseDate = now.AddDays(-16) },

            new TicketEntity { UserId = c1, ConcertId = 20, PurchaseDate = now.AddDays(-14) },
            new TicketEntity { UserId = c2, ConcertId = 20, PurchaseDate = now.AddDays(-14) },
            new TicketEntity { UserId = am2, ConcertId = 20, PurchaseDate = now.AddDays(-14) },

            new TicketEntity { UserId = c1, ConcertId = 21, PurchaseDate = now.AddDays(-12) },
            new TicketEntity { UserId = c2, ConcertId = 21, PurchaseDate = now.AddDays(-12) },
            new TicketEntity { UserId = c4, ConcertId = 21, PurchaseDate = now.AddDays(-12) },

            new TicketEntity { UserId = c1, ConcertId = 22, PurchaseDate = now.AddDays(-10) },
            new TicketEntity { UserId = c2, ConcertId = 22, PurchaseDate = now.AddDays(-10) },
            new TicketEntity { UserId = am1, ConcertId = 22, PurchaseDate = now.AddDays(-10) },

            new TicketEntity { UserId = c1, ConcertId = 23, PurchaseDate = now.AddDays(-8) },
            new TicketEntity { UserId = c2, ConcertId = 23, PurchaseDate = now.AddDays(-8) },
            new TicketEntity { UserId = c5, ConcertId = 23, PurchaseDate = now.AddDays(-8) },

            new TicketEntity { UserId = c1, ConcertId = 24, PurchaseDate = now.AddDays(-6) },
            new TicketEntity { UserId = c2, ConcertId = 24, PurchaseDate = now.AddDays(-6) },
            new TicketEntity { UserId = c4, ConcertId = 24, PurchaseDate = now.AddDays(-6) },

            new TicketEntity { UserId = c1, ConcertId = 25, PurchaseDate = now.AddDays(-4) },
            new TicketEntity { UserId = c2, ConcertId = 25, PurchaseDate = now.AddDays(-4) },
            new TicketEntity { UserId = am2, ConcertId = 25, PurchaseDate = now.AddDays(-4) },

            new TicketEntity { UserId = c1, ConcertId = 26, PurchaseDate = now.AddDays(-2) },
            new TicketEntity { UserId = c2, ConcertId = 26, PurchaseDate = now.AddDays(-2) },
            new TicketEntity { UserId = c5, ConcertId = 26, PurchaseDate = now.AddDays(-2) }
            };

            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();
        }

        // Reviews
        if (!context.Reviews.Any())
        {
            var reviews = new ReviewEntity[]
            {
            new ReviewEntity { TicketId = 1, Stars = 4, Details = "Amazing performance!" },
            new ReviewEntity { TicketId = 2, Stars = 5, Details = "Loved every moment!" },
            new ReviewEntity { TicketId = 3, Stars = 5, Details = "Unforgettable night!" },
            new ReviewEntity { TicketId = 4, Stars = 4, Details = "Great energy from the crowd." },
            new ReviewEntity { TicketId = 5, Stars = 3, Details = "Good, but the sound was a bit off." },
            new ReviewEntity { TicketId = 6, Stars = 5, Details = "Perfect setlist and vibes!" },
            new ReviewEntity { TicketId = 7, Stars = 4, Details = "Would attend again!" },

            new ReviewEntity { TicketId = 8, Stars = 5, Details = "Fantastic indie atmosphere." },
            new ReviewEntity { TicketId = 9, Stars = 4, Details = "Loved the venue!" },
            new ReviewEntity { TicketId = 10, Stars = 4, Details = "Solid performance." },
            new ReviewEntity { TicketId = 11, Stars = 5, Details = "Caught my new favorite artist!" },
            new ReviewEntity { TicketId = 12, Stars = 3, Details = "Good music, but crowded." },
            new ReviewEntity { TicketId = 13, Stars = 5, Details = "Indie dream come true." },
            new ReviewEntity { TicketId = 14, Stars = 4, Details = "Chill night out." },

            new ReviewEntity { TicketId = 15, Stars = 5, Details = "Incredible stage presence!" },
            new ReviewEntity { TicketId = 16, Stars = 4, Details = "Would love to see them again." },
            new ReviewEntity { TicketId = 17, Stars = 5, Details = "Next-level visuals." },
            new ReviewEntity { TicketId = 18, Stars = 4, Details = "Very unique sound." },
            new ReviewEntity { TicketId = 19, Stars = 4, Details = "Great crowd energy." },
            new ReviewEntity { TicketId = 20, Stars = 5, Details = "Absolute fire show." },
            new ReviewEntity { TicketId = 21, Stars = 5, Details = "Perfect DnB experience." },

            new ReviewEntity { TicketId = 22, Stars = 4, Details = "Smooth lyrical vibes." },
            new ReviewEntity { TicketId = 23, Stars = 5, Details = "Top-tier show!" },
            new ReviewEntity { TicketId = 24, Stars = 4, Details = "Nice intimate gig." },
            new ReviewEntity { TicketId = 25, Stars = 3, Details = "A bit too loud but still fun." },
            new ReviewEntity { TicketId = 26, Stars = 4, Details = "Well organized event." },
            new ReviewEntity { TicketId = 27, Stars = 5, Details = "Really enjoyed it." },
            new ReviewEntity { TicketId = 28, Stars = 5, Details = "Brought my friends, all loved it." },

            new ReviewEntity { TicketId = 29, Stars = 3, Details = "Solid but expected more." },
            new ReviewEntity { TicketId = 30, Stars = 4, Details = "The lighting was amazing!" },
            new ReviewEntity { TicketId = 31, Stars = 5, Details = "Instant classic." },
            new ReviewEntity { TicketId = 32, Stars = 4, Details = "Had a great time." },
            new ReviewEntity { TicketId = 33, Stars = 4, Details = "Venue was packed with energy." }
            };
            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }

        if (!context.Transactions.Any())
        {
            var vm1 = GetVenueManager(1);
            var vm2 = GetVenueManager(2);
            var vm3 = GetVenueManager(3);
            var vm4 = GetVenueManager(4);
            var vm5 = GetVenueManager(5);
            var tam1 = GetArtistManager(1);
            var tam2 = GetArtistManager(2);
            var tam3 = GetArtistManager(3);
            var tam4 = GetArtistManager(4);
            var tam5 = GetArtistManager(5);
            var tc1 = GetCustomer(1);
            var tc2 = GetCustomer(2);
            var tc3 = GetCustomer(3);
            var tc4 = GetCustomer(4);
            var tc5 = GetCustomer(5);
            var tc6 = GetCustomer(6);
            var tAm1 = GetArtistManager(1);
            var tAm2 = GetArtistManager(2);
            var tAm3 = GetArtistManager(3);

            var settlementTransactions = new List<SettlementTransactionEntity>
            {
                new SettlementTransactionEntity { CreatedBy = vm1.ToString(), ApplicationId = 1, FromUserId = vm1, ToUserId = tam1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 15000, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-58) },
                new SettlementTransactionEntity { CreatedBy = vm2.ToString(), ApplicationId = 2, FromUserId = vm2, ToUserId = tam2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 20000, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-55) },
                new SettlementTransactionEntity { CreatedBy = vm3.ToString(), ApplicationId = 3, FromUserId = vm3, ToUserId = tam3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 18000, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-52) },
                new SettlementTransactionEntity { CreatedBy = vm4.ToString(), ApplicationId = 4, FromUserId = vm4, ToUserId = tam4, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 17500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-49) },
                new SettlementTransactionEntity { CreatedBy = vm5.ToString(), ApplicationId = 5, FromUserId = vm5, ToUserId = tam5, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 16000, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-46) },
            };

            var ticketTransactions = new List<TicketTransactionEntity>
            {
                new TicketTransactionEntity { CreatedBy = tc1.ToString(),  ConcertId = 1, FromUserId = tc1,  ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-57) },
                new TicketTransactionEntity { CreatedBy = tc2.ToString(),  ConcertId = 1, FromUserId = tc2,  ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-57) },
                new TicketTransactionEntity { CreatedBy = tc3.ToString(),  ConcertId = 1, FromUserId = tc3,  ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-57) },
                new TicketTransactionEntity { CreatedBy = tc4.ToString(),  ConcertId = 1, FromUserId = tc4,  ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-56) },
                new TicketTransactionEntity { CreatedBy = tc5.ToString(),  ConcertId = 1, FromUserId = tc5,  ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-56) },
                new TicketTransactionEntity { CreatedBy = tc6.ToString(),  ConcertId = 1, FromUserId = tc6,  ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-56) },
                new TicketTransactionEntity { CreatedBy = tAm1.ToString(), ConcertId = 1, FromUserId = tAm1, ToUserId = vm1, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1500, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-55) },
                new TicketTransactionEntity { CreatedBy = tc2.ToString(),  ConcertId = 2, FromUserId = tc2,  ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-54) },
                new TicketTransactionEntity { CreatedBy = tc3.ToString(),  ConcertId = 2, FromUserId = tc3,  ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-54) },
                new TicketTransactionEntity { CreatedBy = tc4.ToString(),  ConcertId = 2, FromUserId = tc4,  ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-54) },
                new TicketTransactionEntity { CreatedBy = tc5.ToString(),  ConcertId = 2, FromUserId = tc5,  ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-53) },
                new TicketTransactionEntity { CreatedBy = tc6.ToString(),  ConcertId = 2, FromUserId = tc6,  ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-53) },
                new TicketTransactionEntity { CreatedBy = tAm1.ToString(), ConcertId = 2, FromUserId = tAm1, ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-53) },
                new TicketTransactionEntity { CreatedBy = tAm2.ToString(), ConcertId = 2, FromUserId = tAm2, ToUserId = vm2, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1200, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-52) },
                new TicketTransactionEntity { CreatedBy = tc3.ToString(),  ConcertId = 3, FromUserId = tc3,  ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-51) },
                new TicketTransactionEntity { CreatedBy = tc4.ToString(),  ConcertId = 3, FromUserId = tc4,  ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-51) },
                new TicketTransactionEntity { CreatedBy = tc5.ToString(),  ConcertId = 3, FromUserId = tc5,  ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-51) },
                new TicketTransactionEntity { CreatedBy = tc6.ToString(),  ConcertId = 3, FromUserId = tc6,  ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-50) },
                new TicketTransactionEntity { CreatedBy = tAm1.ToString(), ConcertId = 3, FromUserId = tAm1, ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-50) },
                new TicketTransactionEntity { CreatedBy = tAm2.ToString(), ConcertId = 3, FromUserId = tAm2, ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-50) },
                new TicketTransactionEntity { CreatedBy = tAm3.ToString(), ConcertId = 3, FromUserId = tAm3, ToUserId = vm3, PaymentIntentId = Guid.NewGuid().ToString(), Amount = 1800, Status = TransactionStatus.Complete, CreatedAt = now.AddDays(-49) },
            };

            context.SettlementTransactions.AddRange(settlementTransactions);
            context.TicketTransactions.AddRange(ticketTransactions);
            await context.SaveChangesAsync();
        }

    }
}
