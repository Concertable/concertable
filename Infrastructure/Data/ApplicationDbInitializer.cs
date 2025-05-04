using Core.Entities;
using Core.Entities.Identity;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Data.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ApplicationDbInitializer
    {

        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            await context.Database.MigrateAsync();

            var now = DateTime.UtcNow;
            // Users
            if (!context.Users.Any())
            {
                var locations = LocationList.GetLocations();

                var admin = new Admin
                {
                    UserName = "admin1@test.com",
                    Email = "admin1@test.com",
                    County = "Leicestershire",
                    Town = "Loughborough",
                    EmailConfirmed = true,
                    Location = new Point(-0.5, 51.0) { SRID = 4326 }
                };
                await userManager.CreateAsync(admin, "Password11!");
                await userManager.AddToRoleAsync(admin, "Admin");

                var firstCustomer = CustomerFaker.GetFaker(1, locations[0]).Generate();
                firstCustomer.StripeId = "acct_1R71vrGWdDleGW3a";
                await userManager.CreateAsync(firstCustomer, "Password11!");
                await userManager.AddToRoleAsync(firstCustomer, "Customer");

                for (int i = 2; i <= 6; i++)
                {
                    var location = locations[i % locations.Count];
                    var customer = CustomerFaker.GetFaker(i, location).Generate();
                    await userManager.CreateAsync(customer, "Password11!");
                    await userManager.AddToRoleAsync(customer, "Customer");
                }

                var firstArtistManager = ArtistManagerFaker.GetFaker(1, locations[1]).Generate();
                firstArtistManager.StripeId = "acct_1R71yoLnJh1ZDYF4";
                await userManager.CreateAsync(firstArtistManager, "Password11!");
                await userManager.AddToRoleAsync(firstArtistManager, "ArtistManager");

                var secondArtistManager = ArtistManagerFaker.GetFaker(2, locations[2]).Generate();
                secondArtistManager.StripeId = "acct_1R71z6IBXwkKnqix";
                await userManager.CreateAsync(secondArtistManager, "Password11!");
                await userManager.AddToRoleAsync(secondArtistManager, "ArtistManager");

                for (int i = 3; i <= 35; i++)
                {
                    var location = locations[i % locations.Count];
                    var artistManager = ArtistManagerFaker.GetFaker(i, location).Generate();
                    await userManager.CreateAsync(artistManager, "Password11!");
                    await userManager.AddToRoleAsync(artistManager, "ArtistManager");
                }

                var firstVenueManager = VenueManagerFaker.GetFaker(1, locations[3]).Generate();
                firstVenueManager.StripeId = "acct_1R71zKBsonWwC9oM";
                await userManager.CreateAsync(firstVenueManager, "Password11!");
                await userManager.AddToRoleAsync(firstVenueManager, "VenueManager");

                var secondVenueManager = VenueManagerFaker.GetFaker(2, locations[4]).Generate();
                secondVenueManager.StripeId = "acct_1R71zvLnLloN6AmB";
                await userManager.CreateAsync(secondVenueManager, "Password11!");
                await userManager.AddToRoleAsync(secondVenueManager, "VenueManager");

                for (int i = 3; i <= 35; i++)
                {
                    var location = locations[i % locations.Count];
                    var venueManager = VenueManagerFaker.GetFaker(i, location).Generate();
                    await userManager.CreateAsync(venueManager, "Password11!");
                    await userManager.AddToRoleAsync(venueManager, "VenueManager");
                }
            }

            //Preferences
            if (!context.Preferences.Any())
            {
                var preferences = new Preference[]
                {
                new Preference
                {
                    UserId = 2,
                    RadiusKm = 10
                }
                };
                context.Preferences.AddRange(preferences);
                await context.SaveChangesAsync();
            }

            // Genres
            if (!context.Genres.Any())
            {
                var genres = new Genre[]
                {
                    new Genre { Name = "Rock" },
                    new Genre { Name = "Pop" },
                    new Genre { Name = "Jazz" },
                    new Genre { Name = "Hip-Hop" },
                    new Genre { Name = "Electronic" },
                    new Genre { Name = "Indie" },
                    new Genre { Name = "DnB" },
                    new Genre { Name = "House" }
                };
                context.Genres.AddRange(genres);
                await context.SaveChangesAsync();
            }

            //PreferenceGenres
            if (context.GenrePreferences.Any())
            {
                var genrePreferences = new GenrePreference[]
                {
                        new GenrePreference
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
                var artists = new Artist[]
                {
                    ArtistFaker.GetFaker(8, "The Rockers", "rockers.jpg").Generate(),
                    ArtistFaker.GetFaker(9, "Indie Vibes", "indievibes.jpg").Generate(),
                    ArtistFaker.GetFaker(10, "Electronic Pulse", "electronicpulse.jpg").Generate(),
                    ArtistFaker.GetFaker(11, "Hip-Hop Flow", "hiphopflow.jpg").Generate(),
                    ArtistFaker.GetFaker(12, "Jazz Masters", "jazzmaster.jpg").Generate(),
                    ArtistFaker.GetFaker(13, "Always Punks", "alwayspunks.jpg").Generate(),
                    ArtistFaker.GetFaker(14, "The Hollow Frequencies", "hollowfrequencies.jpg").Generate(),
                    ArtistFaker.GetFaker(15, "Neon Foxes", "neonfoxes.jpg").Generate(),
                    ArtistFaker.GetFaker(16, "Velvet Static", "velvetstatic.jpg").Generate(),
                    ArtistFaker.GetFaker(17, "Echo Bloom", "echobloom.jpg").Generate(),
                    ArtistFaker.GetFaker(18, "The Wild Chords", "wildchords.jpg").Generate(),
                    ArtistFaker.GetFaker(19, "Glitch & Glow", "glitchandglow.jpg").Generate(),
                    ArtistFaker.GetFaker(20, "Sonic Mirage", "sonicmirage.jpg").Generate(),
                    ArtistFaker.GetFaker(21, "Neon Echoes", "neonechoes.jpg").Generate(),
                    ArtistFaker.GetFaker(22, "Dreamwave Collective", "dreamwavecollective.jpg").Generate(),
                    ArtistFaker.GetFaker(23, "Synth Pulse", "synthpulse.jpg").Generate(),
                    ArtistFaker.GetFaker(24, "The Brass Poets", "brasspoets.jpg").Generate(),
                    ArtistFaker.GetFaker(25, "Groove Alchemy", "groovealchemy.jpg").Generate(),
                    ArtistFaker.GetFaker(26, "Velvet Rhymes", "velvetrhymes.jpg").Generate(),
                    ArtistFaker.GetFaker(27, "The Lo-Fi Syndicate", "lofisyndicate.jpg").Generate(),
                    ArtistFaker.GetFaker(28, "Beats & Blue Notes", "beatsbluenotes.jpg").Generate(),
                    ArtistFaker.GetFaker(29, "Bass Pilots", "basspilots.jpg").Generate(),
                    ArtistFaker.GetFaker(30, "The Digital Prophets", "digitalprophets.jpg").Generate(),
                    ArtistFaker.GetFaker(31, "Neon Bass Theory", "neonbasstheory.jpg").Generate(),
                    ArtistFaker.GetFaker(32, "Wavelength 303", "wavelength303.jpg").Generate(),
                    ArtistFaker.GetFaker(33, "Gravity Loops", "gravityloops.jpg").Generate(),
                    ArtistFaker.GetFaker(34, "The Golden Reverie", "goldenreverie.jpg").Generate(),
                    ArtistFaker.GetFaker(35, "Fable Sound", "fablesound.jpg").Generate(),
                    ArtistFaker.GetFaker(36, "Moonlight Static", "moonlightstatic.jpg").Generate(),
                    ArtistFaker.GetFaker(37, "The Chromatics", "thechromatics.jpg").Generate(),
                    ArtistFaker.GetFaker(38, "Echo Reverberation", "echoreverberation.jpg").Generate(),
                    ArtistFaker.GetFaker(39, "Midnight Reverie", "midnightreverie.jpg").Generate(),
                    ArtistFaker.GetFaker(40, "Static Wolves", "staticwolves.jpg").Generate(),
                    ArtistFaker.GetFaker(41, "Echo Collapse", "echocollapse.jpg").Generate(),
                    ArtistFaker.GetFaker(42, "Violet Sundown", "violetsundown.jpg").Generate()
                };
                context.Artists.AddRange(artists);
                await context.SaveChangesAsync();
            }

            // Artist Genres
            if (!context.ArtistGenres.Any())
            {
                var artistGenres = new ArtistGenre[]
                {
                    new ArtistGenre { ArtistId = 1, GenreId = 1 },
                    new ArtistGenre { ArtistId = 1, GenreId = 2 },
                    new ArtistGenre { ArtistId = 1, GenreId = 3 },

                    new ArtistGenre { ArtistId = 2, GenreId = 1 },
                    new ArtistGenre { ArtistId = 2, GenreId = 5 },
                    new ArtistGenre { ArtistId = 2, GenreId = 4 },

                    new ArtistGenre { ArtistId = 3, GenreId = 5 },
                    new ArtistGenre { ArtistId = 3, GenreId = 3 },

                    new ArtistGenre { ArtistId = 4, GenreId = 4 },

                    new ArtistGenre { ArtistId = 5, GenreId = 6 },
                    new ArtistGenre { ArtistId = 5, GenreId = 3 },

                    new ArtistGenre { ArtistId = 6, GenreId = 1 },
                    new ArtistGenre { ArtistId = 6, GenreId = 6 },

                    new ArtistGenre { ArtistId = 7, GenreId = 2 },

                    new ArtistGenre { ArtistId = 8, GenreId = 4 },
                    new ArtistGenre { ArtistId = 8, GenreId = 2 },

                    new ArtistGenre { ArtistId = 9, GenreId = 5 },
                    new ArtistGenre { ArtistId = 9, GenreId = 3 },

                    new ArtistGenre { ArtistId = 10, GenreId = 1 },
                    new ArtistGenre { ArtistId = 10, GenreId = 7 },

                    new ArtistGenre { ArtistId = 11, GenreId = 6 },
                    new ArtistGenre { ArtistId = 11, GenreId = 1 },

                    new ArtistGenre { ArtistId = 12, GenreId = 2 },

                    new ArtistGenre { ArtistId = 13, GenreId = 6 },
                    new ArtistGenre { ArtistId = 13, GenreId = 5 },

                    new ArtistGenre { ArtistId = 14, GenreId = 4 },

                    new ArtistGenre { ArtistId = 15, GenreId = 7 },

                    new ArtistGenre { ArtistId = 16, GenreId = 1 },

                    new ArtistGenre { ArtistId = 17, GenreId = 3 },

                    new ArtistGenre { ArtistId = 18, GenreId = 6 },

                    new ArtistGenre { ArtistId = 19, GenreId = 4 },

                    new ArtistGenre { ArtistId = 20, GenreId = 7 },

                    new ArtistGenre { ArtistId = 21, GenreId = 8 },

                    new ArtistGenre { ArtistId = 22, GenreId = 1 },

                    new ArtistGenre { ArtistId = 23, GenreId = 5 },

                    new ArtistGenre { ArtistId = 24, GenreId = 6 },

                    new ArtistGenre { ArtistId = 25, GenreId = 2 },

                    new ArtistGenre { ArtistId = 26, GenreId = 1 },

                    new ArtistGenre { ArtistId = 27, GenreId = 8 },

                    new ArtistGenre { ArtistId = 28, GenreId = 5 },

                    new ArtistGenre { ArtistId = 29, GenreId = 7 },

                    new ArtistGenre { ArtistId = 30, GenreId = 3 },

                    new ArtistGenre { ArtistId = 31, GenreId = 6 },

                    new ArtistGenre { ArtistId = 32, GenreId = 1 },

                    new ArtistGenre { ArtistId = 33, GenreId = 4 },

                    new ArtistGenre { ArtistId = 34, GenreId = 2 },

                    new ArtistGenre { ArtistId = 35, GenreId = 8 },
                };
                context.ArtistGenres.AddRange(artistGenres);
                await context.SaveChangesAsync();
            }

            // Venues
            if (!context.Venues.Any())
            {
                var venues = new Venue[]
                {
                    VenueFaker.GetFaker(43, "The Grand Venue", "grandvenue.jpg").Generate(), //1
                    VenueFaker.GetFaker(44, "Redhill Hall", "redhillhall.jpg").Generate(), //2
                    VenueFaker.GetFaker(45, "Weybridge Pavilion", "weybridgepavilon.jpg").Generate(), //3
                    VenueFaker.GetFaker(46, "Cobham Arts Centre", "cobhamarts.jpg").Generate(), //4
                    VenueFaker.GetFaker(47, "Chertsey Arena", "chertseyarena.jpg").Generate(), //5
                    VenueFaker.GetFaker(48, "Camden Electric Ballroom", "camdenballroom.jpg").Generate(), //6
                    VenueFaker.GetFaker(49, "Manchester Night & Day Café", "manchesternightday.jpg").Generate(), //7
                    VenueFaker.GetFaker(50, "Birmingham O2 Institute", "birminghamo2.jpg").Generate(), //8
                    VenueFaker.GetFaker(51, "Edinburgh Usher Hall", "edinburghusher.jpg").Generate(), //9
                    VenueFaker.GetFaker(52, "Liverpool Philharmonic Hall", "liverpoolphilharmonic.jpg").Generate(), //10
                    VenueFaker.GetFaker(53, "Leeds Brudenell Social Club", "leedsbrudenell.jpg").Generate(), //11
                    VenueFaker.GetFaker(54, "Glasgow Barrowland Ballroom", "glasgowbarrowland.jpg").Generate(), //12
                    VenueFaker.GetFaker(55, "Sheffield Leadmill", "sheffieldleadmill.jpg").Generate(), //13
                    VenueFaker.GetFaker(56, "Nottingham Rock City", "nottinghamrockcity.jpg").Generate(), //14
                    VenueFaker.GetFaker(57, "Bristol Thekla", "bristolthekla.jpg").Generate(), //15
                    VenueFaker.GetFaker(58, "Brighton Concorde 2", "brightonconcorde2.jpg").Generate(), //16
                    VenueFaker.GetFaker(59, "Cardiff Tramshed", "cardifftramshed.jpg").Generate(), //17
                    VenueFaker.GetFaker(60, "Newcastle O2 Academy", "newcastleo2.jpg").Generate(), //18
                    VenueFaker.GetFaker(61, "Oxford O2 Academy", "oxfordo2.jpg").Generate(), //19
                    VenueFaker.GetFaker(62, "Cambridge Corn Exchange", "cambridgecornexchange.jpg").Generate(), //20
                    VenueFaker.GetFaker(63, "Bath Komedia", "bathkomedia.jpg").Generate(), //21
                    VenueFaker.GetFaker(64, "Aberdeen The Lemon Tree", "aberdeenlemontree.jpg").Generate(), //22
                    VenueFaker.GetFaker(65, "York Barbican", "yorkbarbican.jpg").Generate(), //23
                    VenueFaker.GetFaker(66, "Belfast Limelight", "belfastlimelight.jpg").Generate(), //24
                    VenueFaker.GetFaker(67, "Dublin Vicar Street", "dublinvicarstreet.jpg").Generate(), //25
                    VenueFaker.GetFaker(68, "Norwich Waterfront", "norwichwaterfront.jpg").Generate(), //26
                    VenueFaker.GetFaker(69, "Exeter Phoenix", "exeterphoenix.jpg").Generate(), //27
                    VenueFaker.GetFaker(70, "Southampton Engine Rooms", "southamptonengine.jpg").Generate(), //28
                    VenueFaker.GetFaker(71, "Hull The Welly Club", "hullwellyclub.jpg").Generate(), //29
                    VenueFaker.GetFaker(72, "Plymouth Junction", "plymouthjunction.jpg").Generate(), //30
                    VenueFaker.GetFaker(73, "Swansea Sin City", "swanseasincity.jpg").Generate(), //31
                    VenueFaker.GetFaker(74, "Inverness Ironworks", "invernessironworks.jpg").Generate(), //32
                    VenueFaker.GetFaker(75, "Stirling Albert Halls", "stirlingalberthalls.jpg").Generate(), //33
                    VenueFaker.GetFaker(76, "Dundee Fat Sams", "dundeefatsams.jpg").Generate(), //34
                    VenueFaker.GetFaker(77, "Coventry Empire", "coventryempire.jpg").Generate() //35
                };
                context.Venues.AddRange(venues);
                await context.SaveChangesAsync();
            }

        // Listings
        if (!context.Listings.Any())
        {
            var listings = new Listing[]
            {
                new Listing { VenueId = 1, StartDate = now.AddDays(-60), EndDate = now.AddDays(-60).AddHours(3), Pay = 150 }, //1
                new Listing { VenueId = 2, StartDate = now.AddDays(-55), EndDate = now.AddDays(-55).AddHours(3), Pay = 200 }, //2
                new Listing { VenueId = 3, StartDate = now.AddDays(-50), EndDate = now.AddDays(-50).AddHours(3), Pay = 180 }, //3
                new Listing { VenueId = 4, StartDate = now.AddDays(-45), EndDate = now.AddDays(-45).AddHours(3), Pay = 175 }, //4
                new Listing { VenueId = 5, StartDate = now.AddDays(-40), EndDate = now.AddDays(-40).AddHours(3), Pay = 160 }, //5
                new Listing { VenueId = 6, StartDate = now.AddDays(-35), EndDate = now.AddDays(-35).AddHours(3), Pay = 220 }, //6
                new Listing { VenueId = 7, StartDate = now.AddDays(-30), EndDate = now.AddDays(-30).AddHours(3), Pay = 210 }, //7
                new Listing { VenueId = 8, StartDate = now.AddDays(-25), EndDate = now.AddDays(-25).AddHours(3), Pay = 230 }, //8
                new Listing { VenueId = 9, StartDate = now.AddDays(-20), EndDate = now.AddDays(-20).AddHours(3), Pay = 240 }, //9
                new Listing { VenueId = 10, StartDate = now.AddDays(-15), EndDate = now.AddDays(-15).AddHours(3), Pay = 250 }, //10
                new Listing { VenueId = 1, StartDate = now.AddDays(-10), EndDate = now.AddDays(-10).AddHours(3), Pay = 160 }, //11
                new Listing { VenueId = 2, StartDate = now.AddDays(-5), EndDate = now.AddDays(-5).AddHours(3), Pay = 300 }, //12
                new Listing { VenueId = 3, StartDate = now, EndDate = now.AddHours(3), Pay = 280 }, //13
                new Listing { VenueId = 4, StartDate = now.AddDays(5), EndDate = now.AddDays(5).AddHours(3), Pay = 270 }, //14
                new Listing { VenueId = 5, StartDate = now.AddDays(10), EndDate = now.AddDays(10).AddHours(3), Pay = 265 }, //15
                new Listing { VenueId = 6, StartDate = now.AddDays(15), EndDate = now.AddDays(15).AddHours(3), Pay = 260 }, //16
                new Listing { VenueId = 7, StartDate = now.AddDays(20), EndDate = now.AddDays(20).AddHours(3), Pay = 255 }, //17
                new Listing { VenueId = 8, StartDate = now.AddDays(25), EndDate = now.AddDays(25).AddHours(3), Pay = 250 }, //18
                new Listing { VenueId = 9, StartDate = now.AddDays(30), EndDate = now.AddDays(30).AddHours(3), Pay = 245 }, //19
                new Listing { VenueId = 10, StartDate = now.AddDays(35), EndDate = now.AddDays(35).AddHours(3), Pay = 240 }, //20
                new Listing { VenueId = 1, StartDate = now.AddDays(40), EndDate = now.AddDays(40).AddHours(3), Pay = 235 }, //21
                new Listing { VenueId = 2, StartDate = now.AddDays(45), EndDate = now.AddDays(45).AddHours(3), Pay = 230 }, //22
                new Listing { VenueId = 3, StartDate = now.AddDays(50), EndDate = now.AddDays(50).AddHours(3), Pay = 225 }, //23
                new Listing { VenueId = 4, StartDate = now.AddDays(55), EndDate = now.AddDays(55).AddHours(3), Pay = 220 }, //24
                new Listing { VenueId = 5, StartDate = now.AddDays(60), EndDate = now.AddDays(60).AddHours(3), Pay = 215 }, //25
                new Listing { VenueId = 6, StartDate = now.AddDays(65), EndDate = now.AddDays(65).AddHours(3), Pay = 210 }, //26
                new Listing { VenueId = 7, StartDate = now.AddDays(70), EndDate = now.AddDays(70).AddHours(3), Pay = 205 }, //27
                new Listing { VenueId = 8, StartDate = now.AddDays(75), EndDate = now.AddDays(75).AddHours(3), Pay = 200 }, //28
                new Listing { VenueId = 9, StartDate = now.AddDays(80), EndDate = now.AddDays(80).AddHours(3), Pay = 195 }, //29
                new Listing { VenueId = 10, StartDate = now.AddDays(85), EndDate = now.AddDays(85).AddHours(3), Pay = 190 }, //30
                new Listing { VenueId = 1, StartDate = now.AddDays(85), EndDate = now.AddDays(85).AddHours(3), Pay = 190 }, //31
                new Listing { VenueId = 1, StartDate = now.AddDays(85), EndDate = now.AddDays(85).AddHours(5), Pay = 190 }, //32
                new Listing { VenueId = 1, StartDate = now.AddDays(2), EndDate = now.AddDays(2).AddHours(3), Pay = 150 }, //33
                new Listing { VenueId = 1, StartDate = now.AddDays(4), EndDate = now.AddDays(4).AddHours(3), Pay = 175 }, //34
                new Listing { VenueId = 1, StartDate = now.AddDays(6), EndDate = now.AddDays(6).AddHours(3), Pay = 200 }, //35
                new Listing { VenueId = 2, StartDate = now.AddDays(8), EndDate = now.AddDays(8).AddHours(3), Pay = 150 }, //36
                new Listing { VenueId = 2, StartDate = now.AddDays(10), EndDate = now.AddDays(10).AddHours(3), Pay = 175 }, //37
                new Listing { VenueId = 2, StartDate = now.AddDays(12), EndDate = now.AddDays(12).AddHours(3), Pay = 200 }, //38
                new Listing { VenueId = 3, StartDate = now.AddDays(14), EndDate = now.AddDays(14).AddHours(3), Pay = 150 }, //39
                new Listing { VenueId = 3, StartDate = now.AddDays(16), EndDate = now.AddDays(16).AddHours(3), Pay = 175 }, //40
                new Listing { VenueId = 3, StartDate = now.AddDays(18), EndDate = now.AddDays(18).AddHours(3), Pay = 200 } //41

            };
            context.Listings.AddRange(listings);
            await context.SaveChangesAsync();
        }

        // ListingGenres
        if (!context.ListingGenres.Any())
            {
                var listingGenres = new ListingGenre[]
                {
                    new ListingGenre { ListingId = 1, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 1, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 2, GenreId = 5 }, // Electronic
                    new ListingGenre { ListingId = 3, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 4, GenreId = 4 }, // Hip-Hop
                    new ListingGenre { ListingId = 5, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 5, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 6, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 6, GenreId = 4 }, // Hip-Hop
                    new ListingGenre { ListingId = 7, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 8, GenreId = 4 }, // Hip-Hop
                    new ListingGenre { ListingId = 8, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 9, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 9, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 9, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 10, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 11, GenreId = 5 }, // Electronic
                    new ListingGenre { ListingId = 11, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 12, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 13, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 14, GenreId = 7 }, // DnB
                    new ListingGenre { ListingId = 15, GenreId = 8 }, // House
                    new ListingGenre { ListingId = 16, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 16, GenreId = 7 }, // DnB
                    new ListingGenre { ListingId = 17, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 18, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 19, GenreId = 4 }, // Hip-Hop
                    new ListingGenre { ListingId = 20, GenreId = 7 }, // DnB
                    new ListingGenre { ListingId = 21, GenreId = 8 }, // House
                    new ListingGenre { ListingId = 22, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 22, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 23, GenreId = 5 }, // Electronic
                    new ListingGenre { ListingId = 24, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 25, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 26, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 26, GenreId = 5 }, // Electronic
                    new ListingGenre { ListingId = 27, GenreId = 8 }, // House
                    new ListingGenre { ListingId = 28, GenreId = 5 }, // Electronic
                    new ListingGenre { ListingId = 29, GenreId = 7 }, // DnB
                    new ListingGenre { ListingId = 30, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 30, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 31, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 32, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 33, GenreId = 4 }, // Hip-Hop
                    new ListingGenre { ListingId = 34, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 34, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 35, GenreId = 8 }, // House
                    new ListingGenre { ListingId = 36, GenreId = 6 }, // Indie
                    new ListingGenre { ListingId = 37, GenreId = 7 }, // DnB
                    new ListingGenre { ListingId = 38, GenreId = 3 }, // Jazz
                    new ListingGenre { ListingId = 39, GenreId = 1 }, // Rock
                    new ListingGenre { ListingId = 40, GenreId = 2 }, // Pop
                    new ListingGenre { ListingId = 41, GenreId = 4 }, // Hip-Hop
                    new ListingGenre { ListingId = 41, GenreId = 8 }  // House
};
                context.ListingGenres.AddRange(listingGenres);
                await context.SaveChangesAsync();
            }

        // Listing Applications
        if (!context.ListingApplications.Any())
            {
                var applications = new ListingApplication[]
                {
                    new ListingApplication { ArtistId = 1, ListingId = 1 }, //1
                    new ListingApplication { ArtistId = 2, ListingId = 1 }, //2
                    new ListingApplication { ArtistId = 3, ListingId = 1 }, //3
                    new ListingApplication { ArtistId = 4, ListingId = 1 }, //4

                    new ListingApplication { ArtistId = 1, ListingId = 2 }, //5
                    new ListingApplication { ArtistId = 2, ListingId = 2 }, //6
                    new ListingApplication { ArtistId = 5, ListingId = 2 }, //7
                    new ListingApplication { ArtistId = 6, ListingId = 2 }, //8

                    new ListingApplication { ArtistId = 1, ListingId = 3 }, //9
                    new ListingApplication { ArtistId = 2, ListingId = 3 }, //10
                    new ListingApplication { ArtistId = 7, ListingId = 3 }, //11
                    new ListingApplication { ArtistId = 8, ListingId = 3 }, //12

                    new ListingApplication { ArtistId = 1, ListingId = 4 }, //13
                    new ListingApplication { ArtistId = 2, ListingId = 4 }, //14
                    new ListingApplication { ArtistId = 9, ListingId = 4 }, //15
                    new ListingApplication { ArtistId = 10, ListingId = 4 }, //16

                    new ListingApplication { ArtistId = 1, ListingId = 5 }, //17
                    new ListingApplication { ArtistId = 2, ListingId = 5 }, //18
                    new ListingApplication { ArtistId = 11, ListingId = 5 }, //19
                    new ListingApplication { ArtistId = 12, ListingId = 5 }, //20

                    new ListingApplication { ArtistId = 1, ListingId = 6 }, //21
                    new ListingApplication { ArtistId = 2, ListingId = 6 }, //22
                    new ListingApplication { ArtistId = 13, ListingId = 6 }, //23
                    new ListingApplication { ArtistId = 14, ListingId = 6 }, //24

                    new ListingApplication { ArtistId = 1, ListingId = 7 }, //25
                    new ListingApplication { ArtistId = 2, ListingId = 7 }, //26
                    new ListingApplication { ArtistId = 15, ListingId = 7 }, //27
                    new ListingApplication { ArtistId = 16, ListingId = 7 }, //28

                    new ListingApplication { ArtistId = 1, ListingId = 8 }, //29
                    new ListingApplication { ArtistId = 2, ListingId = 8 }, //30
                    new ListingApplication { ArtistId = 17, ListingId = 8 }, //31
                    new ListingApplication { ArtistId = 18, ListingId = 8 }, //32
                    new ListingApplication { ArtistId = 17, ListingId = 40 }, //31
                    new ListingApplication { ArtistId = 18, ListingId = 41 }, //32

                    new ListingApplication { ArtistId = 1, ListingId = 14 }, //33
                    new ListingApplication { ArtistId = 2, ListingId = 14 }, //34
                    new ListingApplication { ArtistId = 3, ListingId = 14 }, //35
                    new ListingApplication { ArtistId = 4, ListingId = 14 }, //36

                    new ListingApplication { ArtistId = 5, ListingId = 15 }, //37
                    new ListingApplication { ArtistId = 6, ListingId = 15 }, //38
                    new ListingApplication { ArtistId = 7, ListingId = 15 }, //39
                    new ListingApplication { ArtistId = 8, ListingId = 15 }, //40

                    new ListingApplication { ArtistId = 9, ListingId = 16 }, //41
                    new ListingApplication { ArtistId = 10, ListingId = 16 }, //42
                    new ListingApplication { ArtistId = 11, ListingId = 16 }, //43
                    new ListingApplication { ArtistId = 12, ListingId = 16 }, //44

                    new ListingApplication { ArtistId = 13, ListingId = 17 }, //45
                    new ListingApplication { ArtistId = 14, ListingId = 17 }, //46
                    new ListingApplication { ArtistId = 15, ListingId = 17 }, //47
                    new ListingApplication { ArtistId = 16, ListingId = 17 }, //48

                    new ListingApplication { ArtistId = 17, ListingId = 18 }, //49
                    new ListingApplication { ArtistId = 18, ListingId = 18 }, //50
                    new ListingApplication { ArtistId = 19, ListingId = 18 }, //51
                    new ListingApplication { ArtistId = 20, ListingId = 18 }, //52
                };
                context.ListingApplications.AddRange(applications);
                await context.SaveChangesAsync();
            }

                if (!context.Events.Any())
                {
                    var events = new Event[]
                    {
                        EventFaker.GetFaker(1, "The Rockers performing at The Grand Venue", 15m, 120, 80, now.AddDays(-58)).Generate(), //1
                        EventFaker.GetFaker(2, "Indie Vibes performing at Redhill Hall", 12m, 110, 70, now.AddDays(-55)).Generate(), //2
                        EventFaker.GetFaker(3, "Electronic Pulse performing at Weybridge Pavilion", 18m, 130, 100, now.AddDays(-52)).Generate(), //3
                        EventFaker.GetFaker(4, "Hip-Hop Flow performing at Cobham Arts Centre", 10m, 100, 60, now.AddDays(-49)).Generate(), //4
                        EventFaker.GetFaker(5, "Jazz Masters performing at Chertsey Arena", 25m, 140, 110, now.AddDays(-46)).Generate(), //5
                        EventFaker.GetFaker(6, "Always Punks performing at Camden Electric Ballroom", 20m, 150, 90, now.AddDays(-43)).Generate(), //6
                        EventFaker.GetFaker(7, "The Hollow Frequencies performing at Manchester Night & Day Café", 30m, 170, 150, now.AddDays(-40)).Generate(), //7
                        EventFaker.GetFaker(8, "Neon Foxes performing at Birmingham O2 Institute", 16m, 130, 100, now.AddDays(-37)).Generate(), //8
                        EventFaker.GetFaker(9, "Velvet Static performing at Edinburgh Usher Hall", 14m, 115, 75, now.AddDays(-34)).Generate(), //9
                        EventFaker.GetFaker(10, "Echo Bloom performing at Liverpool Philharmonic Hall", 22m, 135, 100, now.AddDays(-31)).Generate(), //10
                        EventFaker.GetFaker(11, "The Wild Chords performing at Leeds Brudenell Social Club", 13m, 125, 85, now.AddDays(-28)).Generate(), //11
                        EventFaker.GetFaker(12, "Glitch & Glow performing at Glasgow Barrowland Ballroom", 11m, 120, 90, now.AddDays(-25)).Generate(), //12
                        EventFaker.GetFaker(13, "Sonic Mirage performing at Sheffield Leadmill", 19m, 140, 110, now.AddDays(-22)).Generate(), //13
                        EventFaker.GetFaker(14, "Neon Echoes performing at Nottingham Rock City", 17m, 135, 105, now.AddDays(-19)).Generate(), //14
                        EventFaker.GetFaker(15, "Dreamwave Collective performing at Bristol Thekla", 21m, 145, 115, now.AddDays(-16)).Generate(), //15
                        EventFaker.GetFaker(16, "Synth Pulse performing at Brighton Concorde 2", 18m, 140, 120, now.AddDays(-13)).Generate(), //16
                        EventFaker.GetFaker(17, "The Brass Poets performing at Cardiff Tramshed", 26m, 155, 130, now.AddDays(-10)).Generate(), //17
                        EventFaker.GetFaker(18, "Groove Alchemy performing at Newcastle O2 Academy", 15m, 120, 100, now.AddDays(-7)).Generate(), //18
                        EventFaker.GetFaker(19, "Velvet Rhymes performing at Oxford O2 Academy", 28m, 160, 145, now.AddDays(-4)).Generate(), //19
                        EventFaker.GetFaker(20, "The Lo-Fi Syndicate performing at Cambridge Corn Exchange", 24m, 150, 130, now.AddDays(-1)).Generate(), //20
                        EventFaker.GetFaker(21, "Beats & Blue Notes performing at Bath Komedia", 27m, 160, 140, now.AddDays(2)).Generate(), //21
                        EventFaker.GetFaker(22, "Bass Pilots performing at Aberdeen The Lemon Tree", 23m, 130, 100, now.AddDays(5)).Generate(), //22
                        EventFaker.GetFaker(23, "The Digital Prophets performing at York Barbican", 29m, 155, 140, now.AddDays(8)).Generate(), //23
                        EventFaker.GetFaker(24, "Neon Bass Theory performing at Belfast Limelight", 10m, 110, 70, now.AddDays(11)).Generate(), //24
                        EventFaker.GetFaker(25, "Wavelength 303 performing at Dublin Vicar Street", 15m, 125, 90, now.AddDays(14)).Generate(), //25
                        EventFaker.GetFaker(26, "Gravity Loops performing at Norwich Waterfront", 30m, 180, 170, now.AddDays(17)).Generate(), //26
                        EventFaker.GetFaker(35, "The Rockers performing at The Grand Venue", 20m, 100, 80, now.AddDays(6)).Generate(), //27
                        EventFaker.GetFaker(39, "Indie Vibes performing at Redhill Hall", 25m, 120, 100, now.AddDays(12)).Generate(), //28
                        EventFaker.GetFaker(42, "Electronic Pulse performing at Weybridge Pavilion", 30m, 140, 120, now.AddDays(18)).Generate(), //29
                        EventFaker.GetFaker(45, "Hip-Hop Flow performing at Cobham Arts Centre", 15m, 100, 80, now.AddDays(22)).Generate(), //30
                        EventFaker.GetFaker(52, "Jazz Masters performing at Chertsey Arena", 20m, 150, 130, now.AddDays(25)).Generate() //31

                    };

                    context.Events.AddRange(events);
                    await context.SaveChangesAsync();
                }

            // EventGenres
            if (!context.EventGenres.Any())
            {
                var eventGenres = new List<EventGenre>
                {
                    new EventGenre { EventId = 1, GenreId = 1 },
                    new EventGenre { EventId = 1, GenreId = 2 },

                    new EventGenre { EventId = 2, GenreId = 2 },
                    new EventGenre { EventId = 2, GenreId = 5 },

                    new EventGenre { EventId = 3, GenreId = 5 },
                    new EventGenre { EventId = 3, GenreId = 3 },

                    new EventGenre { EventId = 4, GenreId = 4 },

                    new EventGenre { EventId = 5, GenreId = 3 },
                    new EventGenre { EventId = 5, GenreId = 6 },
                    new EventGenre { EventId = 5, GenreId = 1 },

                    new EventGenre { EventId = 6, GenreId = 6 },
                    new EventGenre { EventId = 6, GenreId = 4 },

                    new EventGenre { EventId = 7, GenreId = 2 },

                    new EventGenre { EventId = 8, GenreId = 4 },
                    new EventGenre { EventId = 8, GenreId = 1 },

                    new EventGenre { EventId = 9, GenreId = 2 },
                    new EventGenre { EventId = 9, GenreId = 1 },

                    new EventGenre { EventId = 10, GenreId = 6 },

                    new EventGenre { EventId = 11, GenreId = 1 },

                    new EventGenre { EventId = 12, GenreId = 5 },

                    new EventGenre { EventId = 13, GenreId = 4 },

                    new EventGenre { EventId = 14, GenreId = 5 },

                    new EventGenre { EventId = 15, GenreId = 5 },

                    new EventGenre { EventId = 16, GenreId = 5 },

                    new EventGenre { EventId = 17, GenreId = 3 },
                    new EventGenre { EventId = 17, GenreId = 4 },

                    new EventGenre { EventId = 18, GenreId = 3 },
                    new EventGenre { EventId = 18, GenreId = 4 },

                    new EventGenre { EventId = 19, GenreId = 4 },
                    new EventGenre { EventId = 19, GenreId = 3 },

                    new EventGenre { EventId = 20, GenreId = 6 },

                    new EventGenre { EventId = 21, GenreId = 3 },

                    new EventGenre { EventId = 21, GenreId = 4 },

                    new EventGenre { EventId = 22, GenreId = 7 },

                    new EventGenre { EventId = 23, GenreId = 5 },

                    new EventGenre { EventId = 24, GenreId = 7 },

                    new EventGenre { EventId = 25, GenreId = 8 },

                    new EventGenre { EventId = 26, GenreId = 8 }
                };

                context.EventGenres.AddRange(eventGenres);
                await context.SaveChangesAsync();
            }


            // Tickets
            if (!context.Tickets.Any())
        {
            var tickets = new Ticket[]
            {
                new Ticket { UserId = 2, EventId = 1, PurchaseDate = now.AddDays(-58) },
                new Ticket { UserId = 3, EventId = 1, PurchaseDate = now.AddDays(-58) },
                new Ticket { UserId = 4, EventId = 1, PurchaseDate = now.AddDays(-58) },
                new Ticket { UserId = 5, EventId = 1, PurchaseDate = now.AddDays(-57) },
                new Ticket { UserId = 6, EventId = 1, PurchaseDate = now.AddDays(-57) },
                new Ticket { UserId = 7, EventId = 1, PurchaseDate = now.AddDays(-57) },
                new Ticket { UserId = 8, EventId = 1, PurchaseDate = now.AddDays(-56) },

                new Ticket { UserId = 3, EventId = 2, PurchaseDate = now.AddDays(-55) },
                new Ticket { UserId = 4, EventId = 2, PurchaseDate = now.AddDays(-55) },
                new Ticket { UserId = 5, EventId = 2, PurchaseDate = now.AddDays(-55) },
                new Ticket { UserId = 6, EventId = 2, PurchaseDate = now.AddDays(-54) },
                new Ticket { UserId = 7, EventId = 2, PurchaseDate = now.AddDays(-54) },
                new Ticket { UserId = 8, EventId = 2, PurchaseDate = now.AddDays(-54) },
                new Ticket { UserId = 9, EventId = 2, PurchaseDate = now.AddDays(-53) },

                new Ticket { UserId = 4, EventId = 3, PurchaseDate = now.AddDays(-52) },
                new Ticket { UserId = 5, EventId = 3, PurchaseDate = now.AddDays(-52) },
                new Ticket { UserId = 6, EventId = 3, PurchaseDate = now.AddDays(-52) },
                new Ticket { UserId = 7, EventId = 3, PurchaseDate = now.AddDays(-51) },
                new Ticket { UserId = 8, EventId = 3, PurchaseDate = now.AddDays(-51) },
                new Ticket { UserId = 9, EventId = 3, PurchaseDate = now.AddDays(-51) },
                new Ticket { UserId = 10, EventId = 3, PurchaseDate = now.AddDays(-50) },

                new Ticket { UserId = 2, EventId = 4, PurchaseDate = now.AddDays(-49) },
                new Ticket { UserId = 3, EventId = 4, PurchaseDate = now.AddDays(-49) },
                new Ticket { UserId = 4, EventId = 4, PurchaseDate = now.AddDays(-49) },
                new Ticket { UserId = 5, EventId = 4, PurchaseDate = now.AddDays(-48) },
                new Ticket { UserId = 6, EventId = 4, PurchaseDate = now.AddDays(-48) },
                new Ticket { UserId = 7, EventId = 4, PurchaseDate = now.AddDays(-48) },
                new Ticket { UserId = 8, EventId = 4, PurchaseDate = now.AddDays(-47) },

                new Ticket { UserId = 9, EventId = 5, PurchaseDate = now.AddDays(-46) },
                new Ticket { UserId = 10, EventId = 5, PurchaseDate = now.AddDays(-46) },
                new Ticket { UserId = 2, EventId = 5, PurchaseDate = now.AddDays(-46) },
                new Ticket { UserId = 3, EventId = 5, PurchaseDate = now.AddDays(-45) },
                new Ticket { UserId = 4, EventId = 5, PurchaseDate = now.AddDays(-45) },
                new Ticket { UserId = 5, EventId = 5, PurchaseDate = now.AddDays(-45) },
                new Ticket { UserId = 6, EventId = 5, PurchaseDate = now.AddDays(-44) },

                new Ticket { UserId = 2, EventId = 6, PurchaseDate = now.AddDays(-43) },
                new Ticket { UserId = 3, EventId = 6, PurchaseDate = now.AddDays(-43) },
                new Ticket { UserId = 5, EventId = 6, PurchaseDate = now.AddDays(-42) },
                new Ticket { UserId = 6, EventId = 6, PurchaseDate = now.AddDays(-42) },
                new Ticket { UserId = 8, EventId = 6, PurchaseDate = now.AddDays(-42) },

                new Ticket { UserId = 2, EventId = 7, PurchaseDate = now.AddDays(-40) },
                new Ticket { UserId = 3, EventId = 7, PurchaseDate = now.AddDays(-40) },
                new Ticket { UserId = 9, EventId = 7, PurchaseDate = now.AddDays(-40) },

                new Ticket { UserId = 2, EventId = 8, PurchaseDate = now.AddDays(-38) },
                new Ticket { UserId = 3, EventId = 8, PurchaseDate = now.AddDays(-38) },
                new Ticket { UserId = 6, EventId = 8, PurchaseDate = now.AddDays(-37) },

                new Ticket { UserId = 2, EventId = 9, PurchaseDate = now.AddDays(-36) },
                new Ticket { UserId = 3, EventId = 9, PurchaseDate = now.AddDays(-36) },
                new Ticket { UserId = 8, EventId = 9, PurchaseDate = now.AddDays(-36) },

                new Ticket { UserId = 2, EventId = 10, PurchaseDate = now.AddDays(-34) },
                new Ticket { UserId = 3, EventId = 10, PurchaseDate = now.AddDays(-34) },
                new Ticket { UserId = 9, EventId = 10, PurchaseDate = now.AddDays(-34) },

                new Ticket { UserId = 2, EventId = 11, PurchaseDate = now.AddDays(-32) },
                new Ticket { UserId = 3, EventId = 11, PurchaseDate = now.AddDays(-32) },
                new Ticket { UserId = 6, EventId = 11, PurchaseDate = now.AddDays(-32) },

                new Ticket { UserId = 2, EventId = 12, PurchaseDate = now.AddDays(-30) },
                new Ticket { UserId = 3, EventId = 12, PurchaseDate = now.AddDays(-30) },
                new Ticket { UserId = 7, EventId = 12, PurchaseDate = now.AddDays(-30) },

                new Ticket { UserId = 2, EventId = 13, PurchaseDate = now.AddDays(-28) },
                new Ticket { UserId = 3, EventId = 13, PurchaseDate = now.AddDays(-28) },
                new Ticket { UserId = 8, EventId = 13, PurchaseDate = now.AddDays(-28) },

                new Ticket { UserId = 2, EventId = 14, PurchaseDate = now.AddDays(-26) },
                new Ticket { UserId = 3, EventId = 14, PurchaseDate = now.AddDays(-26) },
                new Ticket { UserId = 6, EventId = 14, PurchaseDate = now.AddDays(-26) },

                new Ticket { UserId = 2, EventId = 15, PurchaseDate = now.AddDays(-24) },
                new Ticket { UserId = 3, EventId = 15, PurchaseDate = now.AddDays(-24) },
                new Ticket { UserId = 5, EventId = 15, PurchaseDate = now.AddDays(-24) },

                new Ticket { UserId = 2, EventId = 16, PurchaseDate = now.AddDays(-22) },
                new Ticket { UserId = 3, EventId = 16, PurchaseDate = now.AddDays(-22) },
                new Ticket { UserId = 9, EventId = 16, PurchaseDate = now.AddDays(-22) },

                new Ticket { UserId = 2, EventId = 17, PurchaseDate = now.AddDays(-20) },
                new Ticket { UserId = 3, EventId = 17, PurchaseDate = now.AddDays(-20) },
                new Ticket { UserId = 7, EventId = 17, PurchaseDate = now.AddDays(-20) },

                new Ticket { UserId = 2, EventId = 18, PurchaseDate = now.AddDays(-18) },
                new Ticket { UserId = 3, EventId = 18, PurchaseDate = now.AddDays(-18) },
                new Ticket { UserId = 8, EventId = 18, PurchaseDate = now.AddDays(-18) },

                new Ticket { UserId = 2, EventId = 19, PurchaseDate = now.AddDays(-16) },
                new Ticket { UserId = 3, EventId = 19, PurchaseDate = now.AddDays(-16) },
                new Ticket { UserId = 6, EventId = 19, PurchaseDate = now.AddDays(-16) },

                new Ticket { UserId = 2, EventId = 20, PurchaseDate = now.AddDays(-14) },
                new Ticket { UserId = 3, EventId = 20, PurchaseDate = now.AddDays(-14) },
                new Ticket { UserId = 9, EventId = 20, PurchaseDate = now.AddDays(-14) },

                new Ticket { UserId = 2, EventId = 21, PurchaseDate = now.AddDays(-12) },
                new Ticket { UserId = 3, EventId = 21, PurchaseDate = now.AddDays(-12) },
                new Ticket { UserId = 5, EventId = 21, PurchaseDate = now.AddDays(-12) },

                new Ticket { UserId = 2, EventId = 22, PurchaseDate = now.AddDays(-10) },
                new Ticket { UserId = 3, EventId = 22, PurchaseDate = now.AddDays(-10) },
                new Ticket { UserId = 8, EventId = 22, PurchaseDate = now.AddDays(-10) },

                new Ticket { UserId = 2, EventId = 23, PurchaseDate = now.AddDays(-8) },
                new Ticket { UserId = 3, EventId = 23, PurchaseDate = now.AddDays(-8) },
                new Ticket { UserId = 6, EventId = 23, PurchaseDate = now.AddDays(-8) },

                new Ticket { UserId = 2, EventId = 24, PurchaseDate = now.AddDays(-6) },
                new Ticket { UserId = 3, EventId = 24, PurchaseDate = now.AddDays(-6) },
                new Ticket { UserId = 5, EventId = 24, PurchaseDate = now.AddDays(-6) },

                new Ticket { UserId = 2, EventId = 25, PurchaseDate = now.AddDays(-4) },
                new Ticket { UserId = 3, EventId = 25, PurchaseDate = now.AddDays(-4) },
                new Ticket { UserId = 9, EventId = 25, PurchaseDate = now.AddDays(-4) },

                new Ticket { UserId = 2, EventId = 26, PurchaseDate = now.AddDays(-2) },
                new Ticket { UserId = 3, EventId = 26, PurchaseDate = now.AddDays(-2) },
                new Ticket { UserId = 6, EventId = 26, PurchaseDate = now.AddDays(-2) }
            };

            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();
        }

        // Reviews
        if (!context.Reviews.Any())
        {
            var reviews = new Review[]
            {
                new Review { TicketId = 1, Stars = 4, Details = "Amazing performance!" },
                new Review { TicketId = 2, Stars = 5, Details = "Loved every moment!" },
                new Review { TicketId = 3, Stars = 5, Details = "Unforgettable night!" },
                new Review { TicketId = 4, Stars = 4, Details = "Great energy from the crowd." },
                new Review { TicketId = 5, Stars = 3, Details = "Good, but the sound was a bit off." },
                new Review { TicketId = 6, Stars = 5, Details = "Perfect setlist and vibes!" },
                new Review { TicketId = 7, Stars = 4, Details = "Would attend again!" },

                new Review { TicketId = 8, Stars = 5, Details = "Fantastic indie atmosphere." },
                new Review { TicketId = 9, Stars = 4, Details = "Loved the venue!" },
                new Review { TicketId = 10, Stars = 4, Details = "Solid performance." },
                new Review { TicketId = 11, Stars = 5, Details = "Caught my new favorite artist!" },
                new Review { TicketId = 12, Stars = 3, Details = "Good music, but crowded." },
                new Review { TicketId = 13, Stars = 5, Details = "Indie dream come true." },
                new Review { TicketId = 14, Stars = 4, Details = "Chill night out." },

                new Review { TicketId = 15, Stars = 5, Details = "Incredible stage presence!" },
                new Review { TicketId = 16, Stars = 4, Details = "Would love to see them again." },
                new Review { TicketId = 17, Stars = 5, Details = "Next-level visuals." },
                new Review { TicketId = 18, Stars = 4, Details = "Very unique sound." },
                new Review { TicketId = 19, Stars = 4, Details = "Great crowd energy." },
                new Review { TicketId = 20, Stars = 5, Details = "Absolute fire show." },
                new Review { TicketId = 21, Stars = 5, Details = "Perfect DnB experience." },

                new Review { TicketId = 22, Stars = 4, Details = "Smooth lyrical vibes." },
                new Review { TicketId = 23, Stars = 5, Details = "Top-tier show!" },
                new Review { TicketId = 24, Stars = 4, Details = "Nice intimate gig." },
                new Review { TicketId = 25, Stars = 3, Details = "A bit too loud but still fun." },
                new Review { TicketId = 26, Stars = 4, Details = "Well organized event." },
                new Review { TicketId = 27, Stars = 5, Details = "Really enjoyed it." },
                new Review { TicketId = 28, Stars = 5, Details = "Brought my friends, all loved it." },

                new Review { TicketId = 29, Stars = 3, Details = "Solid but expected more." },
                new Review { TicketId = 30, Stars = 4, Details = "The lighting was amazing!" },
                new Review { TicketId = 31, Stars = 5, Details = "Instant classic." },
                new Review { TicketId = 32, Stars = 4, Details = "Had a great time." },
                new Review { TicketId = 33, Stars = 4, Details = "Venue was packed with energy." }
            };
            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }

            if (!context.Transactions.Any())
            {
                var transactions = new List<Transaction>
                {
                    new Transaction { FromUserId = 43, ToUserId = 8, TransactionId = Guid.NewGuid().ToString(), Amount = 150, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-58) },
                    new Transaction { FromUserId = 44, ToUserId = 9, TransactionId = Guid.NewGuid().ToString(), Amount = 200, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-55) },
                    new Transaction { FromUserId = 45, ToUserId = 10, TransactionId = Guid.NewGuid().ToString(), Amount = 180, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-52) },
                    new Transaction { FromUserId = 46, ToUserId = 11, TransactionId = Guid.NewGuid().ToString(), Amount = 175, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-49) },
                    new Transaction { FromUserId = 47, ToUserId = 12, TransactionId = Guid.NewGuid().ToString(), Amount = 160, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-46) },

                    new Transaction { FromUserId = 2, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-57) },
                    new Transaction { FromUserId = 3, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-57) },
                    new Transaction { FromUserId = 4, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-57) },
                    new Transaction { FromUserId = 5, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-56) },
                    new Transaction { FromUserId = 6, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-56) },
                    new Transaction { FromUserId = 7, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-56) },
                    new Transaction { FromUserId = 8, ToUserId = 43, TransactionId = Guid.NewGuid().ToString(), Amount = 15, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-55) },
                    new Transaction { FromUserId = 3, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-54) },
                    new Transaction { FromUserId = 4, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-54) },
                    new Transaction { FromUserId = 5, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-54) },
                    new Transaction { FromUserId = 6, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-53) },
                    new Transaction { FromUserId = 7, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-53) },
                    new Transaction { FromUserId = 8, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-53) },
                    new Transaction { FromUserId = 9, ToUserId = 44, TransactionId = Guid.NewGuid().ToString(), Amount = 12, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-52) },
                    new Transaction { FromUserId = 4, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-51) },
                    new Transaction { FromUserId = 5, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-51) },
                    new Transaction { FromUserId = 6, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-51) },
                    new Transaction { FromUserId = 7, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-50) },
                    new Transaction { FromUserId = 8, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-50) },
                    new Transaction { FromUserId = 9, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-50) },
                    new Transaction { FromUserId = 10, ToUserId = 45, TransactionId = Guid.NewGuid().ToString(), Amount = 18, Type = "ticket", Status = "Completed", CreatedAt = now.AddDays(-49) },
                };

                context.Transactions.AddRange(transactions);
                await context.SaveChangesAsync();
            }

        }
    }
}
