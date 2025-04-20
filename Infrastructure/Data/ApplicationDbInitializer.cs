using Core.Entities;
using Core.Entities.Identity;
using Core.Parameters;
using Infrastructure.Data.Identity;
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
                var users = new ApplicationUser[]
                {
                    // Admins
                    new Admin { UserName = "admin1@test.com", Email = "admin1@test.com", County = "Surrey", Town = "Woking", EmailConfirmed = true, StripeId = "acct_1R71v9Bt1TeOQuP6", Location = new Point(-0.5, 51.0) { SRID = 4326 } }, //1
                    // Customers
                    new Customer { UserName = "customer1@test.com", Email = "customer1@test.com", County = "Surrey", Town = "Guildford", EmailConfirmed = true, StripeId = "acct_1R71vrGWdDleGW3a", Location = new Point(-0.56, 51.25) { SRID = 4326 } }, //2
                    new Customer { UserName = "customer2@test.com", Email = "customer2@test.com", County = "Surrey", Town = "Epsom", EmailConfirmed = true, Location = new Point(-0.27, 51.34) { SRID = 4326 } }, //3
                    new Customer { UserName = "customer3@test.com", Email = "customer3@test.com", County = "London", Town = "Camden", EmailConfirmed = true, Location = new Point(-0.13, 51.53) { SRID = 4326 } }, //4
                    new Customer { UserName = "customer4@test.com", Email = "customer4@test.com", County = "Edinburgh", Town = "Leith", EmailConfirmed = true, Location = new Point(-3.17, 55.98) { SRID = 4326 } }, //5
                    new Customer { UserName = "customer5@test.com", Email = "customer5@test.com", County = "Sheffield", Town = "Kelham Island", EmailConfirmed = true, Location = new Point(-1.46, 53.39) { SRID = 4326 } }, //6
                    new Customer { UserName = "customer6@test.com", Email = "customer6@test.com", County = "Bath", Town = "Widcombe", EmailConfirmed = true, Location = new Point(-2.36, 51.38) { SRID = 4326 } }, //7
                    // Artist Managers
                    new ArtistManager { UserName = "artistmanager1@test.com", Email = "artistmanager1@test.com", County = "Surrey", Town = "Dorking", EmailConfirmed = true, StripeId = "acct_1R71yoLnJh1ZDYF4", Location = new Point(-0.33, 51.23) { SRID = 4326 } }, //8
                    new ArtistManager { UserName = "artistmanager2@test.com", Email = "artistmanager2@test.com", County = "Surrey", Town = "Reigate", EmailConfirmed = true, StripeId = "acct_1R71z6IBXwkKnqix", Location = new Point(-0.17, 51.23) { SRID = 4326 } }, //9
                    new ArtistManager { UserName = "artistmanager3@test.com", Email = "artistmanager3@test.com", County = "Surrey", Town = "Farnham", EmailConfirmed = true, Location = new Point(-0.58, 51.21) { SRID = 4326 } }, //10
                    new ArtistManager { UserName = "artistmanager4@test.com", Email = "artistmanager4@test.com", County = "Surrey", Town = "Camberley", EmailConfirmed = true, Location = new Point(-0.7, 51.34) { SRID = 4326 } }, //11
                    new ArtistManager { UserName = "artistmanager5@test.com", Email = "artistmanager5@test.com", County = "Surrey", Town = "Haslemere", EmailConfirmed = true, Location = new Point(-0.74, 51.08) { SRID = 4326 } }, //12
                    new ArtistManager { UserName = "artistmanager6@test.com", Email = "artistmanager6@test.com", County = "London", Town = "Camden", EmailConfirmed = true, Location = new Point(-0.13, 51.53) { SRID = 4326 } }, //13
                    new ArtistManager { UserName = "artistmanager7@test.com", Email = "artistmanager7@test.com", County = "Manchester", Town = "Salford", EmailConfirmed = true, Location = new Point(-2.25, 53.48) { SRID = 4326 } }, //14
                    new ArtistManager { UserName = "artistmanager8@test.com", Email = "artistmanager8@test.com", County = "Birmingham", Town = "Digbeth", EmailConfirmed = true, Location = new Point(-1.88, 52.47) { SRID = 4326 } }, //15
                    new ArtistManager { UserName = "artistmanager9@test.com", Email = "artistmanager9@test.com", County = "Edinburgh", Town = "Leith", EmailConfirmed = true, Location = new Point(-3.17, 55.98) { SRID = 4326 } }, //16
                    new ArtistManager { UserName = "artistmanager10@test.com", Email = "artistmanager10@test.com", County = "Liverpool", Town = "Baltic Triangle", EmailConfirmed = true, Location = new Point(-2.98, 53.39) { SRID = 4326 } }, //17
                    new ArtistManager { UserName = "artistmanager11@test.com", Email = "artistmanager11@test.com", County = "Leeds", Town = "Headingley", EmailConfirmed = true, Location = new Point(-1.58, 53.82) { SRID = 4326 } }, //18
                    new ArtistManager { UserName = "artistmanager12@test.com", Email = "artistmanager12@test.com", County = "Glasgow", Town = "West End", EmailConfirmed = true, Location = new Point(-4.29, 55.87) { SRID = 4326 } }, //19
                    new ArtistManager { UserName = "artistmanager13@test.com", Email = "artistmanager13@test.com", County = "Sheffield", Town = "Kelham Island", EmailConfirmed = true, Location = new Point(-1.46, 53.39) { SRID = 4326 } }, //20
                    new ArtistManager { UserName = "artistmanager14@test.com", Email = "artistmanager14@test.com", County = "Nottingham", Town = "Lace Market", EmailConfirmed = true, Location = new Point(-1.14, 52.95) { SRID = 4326 } }, //21
                    new ArtistManager { UserName = "artistmanager15@test.com", Email = "artistmanager150@test.com", County = "Bristol", Town = "Stokes Croft", EmailConfirmed = true, Location = new Point(-2.59, 51.46) { SRID = 4326 } }, //22
                    new ArtistManager { UserName = "artistmanager16@test.com", Email = "artistmanager16@test.com", County = "Brighton", Town = "Kemptown", EmailConfirmed = true, Location = new Point(-0.13, 50.82) { SRID = 4326 } }, //23
                    new ArtistManager { UserName = "artistmanager17@test.com", Email = "artistmanager17@test.com", County = "Cardiff", Town = "Cathays", EmailConfirmed = true, Location = new Point(-3.17, 51.49) { SRID = 4326 } }, //24
                    new ArtistManager { UserName = "artistmanager18@test.com", Email = "artistmanager18@test.com", County = "Newcastle", Town = "Jesmond", EmailConfirmed = true, Location = new Point(-1.61, 54.99) { SRID = 4326 } }, //25
                    new ArtistManager { UserName = "artistmanager19@test.com", Email = "artistmanager19@test.com", County = "Oxford", Town = "Jericho", EmailConfirmed = true, Location = new Point(-1.26, 51.76) { SRID = 4326 } }, //26
                    new ArtistManager { UserName = "artistmanager20@test.com", Email = "artistmanager20@test.com", County = "Cambridge", Town = "Mill Road", EmailConfirmed = true, Location = new Point(0.13, 52.19) { SRID = 4326 } }, //27
                    new ArtistManager { UserName = "artistmanager21@test.com", Email = "artistmanager21@test.com", County = "Bath", Town = "Widcombe", EmailConfirmed = true, Location = new Point(-2.36, 51.38) { SRID = 4326 } }, //28
                    new ArtistManager { UserName = "artistmanager22@test.com", Email = "artistmanager22@test.com", County = "Aberdeen", Town = "Old Aberdeen", EmailConfirmed = true, Location = new Point(-2.1, 57.17) { SRID = 4326 } },  //29
                    new ArtistManager { UserName = "artistmanager23@test.com", Email = "artistmanager23@test.com", County = "York", Town = "The Shambles", EmailConfirmed = true, Location = new Point(-1.08, 53.96) { SRID = 4326 } }, //30
                    new ArtistManager { UserName = "artistmanager24@test.com", Email = "artistmanager24@test.com", County = "Belfast", Town = "Cathedral Quarter", EmailConfirmed = true, Location = new Point(-5.93, 54.6) { SRID = 4326 } }, //31
                    new ArtistManager { UserName = "artistmanager25@test.com", Email = "artistmanager25@test.com", County = "Dublin", Town = "Temple Bar", EmailConfirmed = true, Location = new Point(-6.27, 53.34) { SRID = 4326 } }, //32
                    new ArtistManager { UserName = "artistmanager26@test.com", Email = "artistmanager26@test.com", County = "Norwich", Town = "Tombland", EmailConfirmed = true, Location = new Point(1.3, 52.63) { SRID = 4326 } }, //33
                    new ArtistManager { UserName = "artistmanager27@test.com", Email = "artistmanager27@test.com", County = "Exeter", Town = "St Sidwell's", EmailConfirmed = true, Location = new Point(-3.53, 50.73) { SRID = 4326 } }, //34
                    new ArtistManager { UserName = "artistmanager28@test.com", Email = "artistmanager28@test.com", County = "Southampton", Town = "Ocean Village", EmailConfirmed = true, Location = new Point(-1.4, 50.9) { SRID = 4326 } }, //35
                    new ArtistManager { UserName = "artistmanager29@test.com", Email = "artistmanager29@test.com", County = "Hull", Town = "Old Town", EmailConfirmed = true, Location = new Point(-0.33, 53.74) { SRID = 4326 } }, //36
                    new ArtistManager { UserName = "artistmanager30@test.com", Email = "artistmanager30@test.com", County = "Plymouth", Town = "The Hoe", EmailConfirmed = true, Location = new Point(-4.14, 50.37) { SRID = 4326 } }, //37
                    new ArtistManager { UserName = "artistmanager31@test.com", Email = "artistmanager31@test.com", County = "Swansea", Town = "Uplands", EmailConfirmed = true, Location = new Point(-3.94, 51.62) { SRID = 4326 } }, //38
                    new ArtistManager { UserName = "artistmanager32@test.com", Email = "artistmanager32@test.com", County = "Inverness", Town = "Dalneigh", EmailConfirmed = true, Location = new Point(-4.23, 57.48) { SRID = 4326 } }, //39
                    new ArtistManager { UserName = "artistmanager33@test.com", Email = "artistmanager33@test.com", County = "Stirling", Town = "Bridge of Allan", EmailConfirmed = true, Location = new Point(-3.93, 56.15) { SRID = 4326 } }, //40
                    new ArtistManager { UserName = "artistmanager34@test.com", Email = "artistmanager34@test.com", County = "Dundee", Town = "Broughty Ferry", EmailConfirmed = true, Location = new Point(-2.87, 56.47) { SRID = 4326 } }, //41
                    new ArtistManager { UserName = "artistmanager35@test.com", Email = "artistmanager35@test.com", County = "Coventry", Town = "Earlsdon", EmailConfirmed = true, Location = new Point(-1.52, 52.4) { SRID = 4326 } }, //42
                    // Venue Managers
                    new VenueManager { UserName = "venuemanager1@test.com", Email = "venuemanager1@test.com", County = "Surrey", Town = "Leatherhead", EmailConfirmed = true, StripeId = "acct_1R71zKBsonWwC9oM", Location = new Point(-0.3, 51.3) { SRID = 4326 } }, //43
                    new VenueManager { UserName = "venuemanager2@test.com", Email = "venuemanager2@test.com", County = "Surrey", Town = "Redhill", EmailConfirmed = true, StripeId = "acct_1R71zvLnLloN6AmB", Location = new Point(-0.17, 51.23) { SRID = 4326 } }, //44
                    new VenueManager { UserName = "venuemanager3@test.com", Email = "venuemanager3@test.com", County = "Surrey", Town = "Weybridge", EmailConfirmed = true, Location = new Point(-0.46, 51.38) { SRID = 4326 } }, //45
                    new VenueManager { UserName = "venuemanager4@test.com", Email = "venuemanager4@test.com", County = "Surrey", Town = "Cobham", EmailConfirmed = true, Location = new Point(-0.46, 51.32) { SRID = 4326 } }, //46
                    new VenueManager { UserName = "venuemanager5@test.com", Email = "venuemanager5@test.com", County = "Surrey", Town = "Chertsey", EmailConfirmed = true, Location = new Point(-0.5, 51.39) { SRID = 4326 } }, //47
                    new VenueManager { UserName = "venuemanager6@test.com", Email = "venuemanager6@test.com", County = "London", Town = "Camden", EmailConfirmed = true, Location = new Point(-0.13, 51.53) { SRID = 4326 } }, //48
                    new VenueManager { UserName = "venuemanager7@test.com", Email = "venuemanager7@test.com", County = "Manchester", Town = "Northern Quarter", EmailConfirmed = true, Location = new Point(-2.23, 53.48) { SRID = 4326 } }, //49
                    new VenueManager { UserName = "venuemanager8@test.com", Email = "venuemanager8@test.com", County = "Birmingham", Town = "Jewellery Quarter", EmailConfirmed = true, Location = new Point(-1.91, 52.48) { SRID = 4326 } }, //50
                    new VenueManager { UserName = "venuemanager9@test.com", Email = "venuemanager9@test.com", County = "Edinburgh", Town = "Old Town", EmailConfirmed = true, Location = new Point(-3.19, 55.95) { SRID = 4326 } }, //51
                    new VenueManager { UserName = "venuemanager10@test.com", Email = "venuemanager10@test.com", County = "Liverpool", Town = "Cavern Quarter", EmailConfirmed = true, Location = new Point(-2.99, 53.41) { SRID = 4326 } }, //52
                    new VenueManager { UserName = "venuemanager11@test.com", Email = "venuemanager11@test.com", County = "Leeds", Town = "Call Lane", EmailConfirmed = true, Location = new Point(-1.54, 53.79) { SRID = 4326 } }, //53
                    new VenueManager { UserName = "venuemanager12@test.com", Email = "venuemanager12@test.com", County = "Glasgow", Town = "Merchant City", EmailConfirmed = true, Location = new Point(-4.24, 55.86) { SRID = 4326 } }, //54
                    new VenueManager { UserName = "venuemanager13@test.com", Email = "venuemanager13@test.com", County = "Sheffield", Town = "Ecclesall Road", EmailConfirmed = true, Location = new Point(-1.5, 53.38) { SRID = 4326 } }, //55
                    new VenueManager { UserName = "venuemanager14@test.com", Email = "venuemanager14@test.com", County = "Nottingham", Town = "Hockley", EmailConfirmed = true, Location = new Point(-1.14, 52.95) { SRID = 4326 } }, //56
                    new VenueManager { UserName = "venuemanager15@test.com", Email = "venuemanager15@test.com", County = "Bristol", Town = "Harbourside", EmailConfirmed = true, Location = new Point(-2.6, 51.45) { SRID = 4326 } }, //57
                    new VenueManager { UserName = "venuemanager16@test.com", Email = "venuemanager16@test.com", County = "Brighton", Town = "The Lanes", EmailConfirmed = true, Location = new Point(-0.14, 50.82) { SRID = 4326 } }, //58
                    new VenueManager { UserName = "venuemanager17@test.com", Email = "venuemanager17@test.com", County = "Cardiff", Town = "Riverside", EmailConfirmed = true, Location = new Point(-3.18, 51.48) { SRID = 4326 } }, //59
                    new VenueManager { UserName = "venuemanager18@test.com", Email = "venuemanager18@test.com", County = "Newcastle", Town = "Quayside", EmailConfirmed = true, Location = new Point(-1.6, 54.97) { SRID = 4326 } }, //60
                    new VenueManager { UserName = "venuemanager19@test.com", Email = "venuemanager19@test.com", County = "Oxford", Town = "Cowley", EmailConfirmed = true, Location = new Point(-1.22, 51.73) { SRID = 4326 } }, //61
                    new VenueManager { UserName = "venuemanager20@test.com", Email = "venuemanager20@test.com", County = "Cambridge", Town = "Chesterton", EmailConfirmed = true, Location = new Point(0.14, 52.22) { SRID = 4326 } }, //62
                    new VenueManager { UserName = "venuemanager21@test.com", Email = "venuemanager21@test.com", County = "Bath", Town = "Bear Flat", EmailConfirmed = true, Location = new Point(-2.36, 51.37) { SRID = 4326 } }, //63
                    new VenueManager { UserName = "venuemanager22@test.com", Email = "venuemanager22@test.com", County = "Aberdeen", Town = "Footdee", EmailConfirmed = true, Location = new Point(-2.08, 57.15) { SRID = 4326 } }, //64
                    new VenueManager { UserName = "venuemanager23@test.com", Email = "venuemanager23@test.com", County = "York", Town = "Fossgate", EmailConfirmed = true, Location = new Point(-1.08, 53.96) { SRID = 4326 } }, //65
                    new VenueManager { UserName = "venuemanager24@test.com", Email = "venuemanager24@test.com", County = "Belfast", Town = "Titanic Quarter", EmailConfirmed = true, Location = new Point(-5.91, 54.61) { SRID = 4326 } }, //66
                    new VenueManager { UserName = "venuemanager25@test.com", Email = "venuemanager25@test.com", County = "Dublin", Town = "Grafton Street", EmailConfirmed = true, Location = new Point(-6.26, 53.34) { SRID = 4326 } }, //67
                    new VenueManager { UserName = "venuemanager26@test.com", Email = "venuemanager26@test.com", County = "Norwich", Town = "Magdalen Street", EmailConfirmed = true, Location = new Point(1.3, 52.63) { SRID = 4326 } }, //68
                    new VenueManager { UserName = "venuemanager27@test.com", Email = "venuemanager27@test.com", County = "Exeter", Town = "Quay", EmailConfirmed = true, Location = new Point(-3.53, 50.72) { SRID = 4326 } }, //69
                    new VenueManager { UserName = "venuemanager28@test.com", Email = "venuemanager28@test.com", County = "Southampton", Town = "Bargate", EmailConfirmed = true, Location = new Point(-1.4, 50.9) { SRID = 4326 } }, //70
                    new VenueManager { UserName = "venuemanager29@test.com", Email = "venuemanager29@test.com", County = "Hull", Town = "Fruit Market", EmailConfirmed = true, Location = new Point(-0.34, 53.74) { SRID = 4326 } }, //71
                    new VenueManager { UserName = "venuemanager30@test.com", Email = "venuemanager30@test.com", County = "Plymouth", Town = "Barbican", EmailConfirmed = true, Location = new Point(-4.14, 50.37) { SRID = 4326 } }, //72
                    new VenueManager { UserName = "venuemanager31@test.com", Email = "venuemanager31@test.com", County = "Swansea", Town = "Mumbles", EmailConfirmed = true, Location = new Point(-3.98, 51.58) { SRID = 4326 }  }, //73
                    new VenueManager { UserName = "venuemanager32@test.com", Email = "venuemanager32@test.com", County = "Inverness", Town = "Crown", EmailConfirmed = true, Location = new Point(-4.23, 57.48) { SRID = 4326 } }, //74
                    new VenueManager { UserName = "venuemanager33@test.com", Email = "venuemanager33@test.com", County = "Stirling", Town = "Causewayhead", EmailConfirmed = true, Location = new Point(-3.93, 56.15) { SRID = 4326 } }, //75
                    new VenueManager { UserName = "venuemanager34@test.com", Email = "venuemanager34@test.com", County = "Dundee", Town = "Seagate", EmailConfirmed = true, Location = new Point(-2.87, 56.47) { SRID = 4326 } }, //76
                    new VenueManager { UserName = "venuemanager35@test.com", Email = "venuemanager35@test.com", County = "Coventry", Town = "Far Gosford Street", EmailConfirmed = true, Location = new Point(-1.5, 52.41) { SRID = 4326 } }, //77   
                    
                    new ArtistManager { UserName = "dummyartistmanager1@test.com", Email = "dummyartistmanager1@test.com", County = "Coventry", Town = "Earlsdon", EmailConfirmed = true, Location = new Point(-1.52, 52.4) { SRID = 4326 } }, //42
                    new ArtistManager { UserName = "dummyartistmanager2@test.com", Email = "dummyartistmanager2@test.com", County = "Coventry", Town = "Earlsdon", EmailConfirmed = true, Location = new Point(-1.52, 52.4) { SRID = 4326 } }, //42
                    new ArtistManager { UserName = "dummyartistmanager3@test.com", Email = "dummyartistmanager3@test.com", County = "Coventry", Town = "Earlsdon", EmailConfirmed = true, Location = new Point(-1.52, 52.4) { SRID = 4326 } }, //42
                    new VenueManager { UserName = "dummyvenuemanager1@test.com", Email = "dummyvenuemanager1@test.com", County = "Surrey", Town = "Leatherhead", EmailConfirmed = true, Location = new Point(-0.3, 51.3) { SRID = 4326 } },
                    new VenueManager { UserName = "dummyvenuemanager2@test.com", Email = "dummyvenuemanager2@test.com", County = "Surrey", Town = "Leatherhead", EmailConfirmed = true, Location = new Point(-0.3, 51.3) { SRID = 4326 } },
                    new VenueManager { UserName = "dummyvenuemanager3@test.com", Email = "dummyvenuemanager3@test.com", County = "Surrey", Town = "Leatherhead", EmailConfirmed = true, Location = new Point(-0.3, 51.3) { SRID = 4326 } },
                    new Customer { UserName = "dummycustomer1@test.com", Email = "dummycustomer1@test.com", County = "Bath", Town = "Widcombe", EmailConfirmed = true, Location = new Point(-2.36, 51.38) { SRID = 4326 } }, //7
                    new Customer { UserName = "dummycustomer2@test.com", Email = "dummycustomer2@test.com", County = "Bath", Town = "Widcombe", EmailConfirmed = true, Location = new Point(-2.36, 51.38) { SRID = 4326 } }, //7
                    new Customer { UserName = "dummycustomer3@test.com", Email = "dummycustomer3@test.com", County = "Bath", Town = "Widcombe", EmailConfirmed = true, Location = new Point(-2.36, 51.38) { SRID = 4326 } }, //7
                };


                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Password11!");
                    await userManager.AddToRoleAsync(user, user.GetType().Name.Replace("ApplicationUser", ""));
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
                new Artist { UserId = 8, Name = "The Rockers", About = "A high-energy rock band delivering explosive guitar riffs, thunderous drums, and electrifying live performances. With a sound rooted in classic rock and modern alternative influences, they bring anthems that shake stadiums and ignite crowds.", ImageUrl = "rockers.jpg" }, //1
                new Artist { UserId = 9, Name = "Indie Vibes", About = "A soulful and melodic indie band known for their breezy guitar tones, heartfelt lyrics, and chill yet uplifting soundscapes. Whether it’s a laid-back acoustic ballad or an upbeat indie anthem, their music is a perfect soundtrack for lazy afternoons and late-night nostalgia.", ImageUrl = "indievibes.jpg" }, //2
                new Artist { UserId = 10, Name = "Electronic Pulse", About = "A high-energy electronic music act, crafting pulsating beats, euphoric synth drops, and infectious dancefloor anthems. Their sound fuses house, trance, and EDM, making every track a sonic rush built for club nights and festival stages.", ImageUrl = "electronicpulse.jpg" }, //3
                new Artist { UserId = 11, Name = "Hip-Hop Flow", About = "A rhythm-driven hip-hop collective, blending smooth lyrical storytelling, deep basslines, and head-nodding beats. Their music is a mix of classic boom-bap, modern trap, and jazz-infused hip-hop, perfect for lovers of authentic and dynamic flows.", ImageUrl = "hiphopflow.jpg" }, //4
                new Artist { UserId = 12, Name = "Jazz Masters", About = "A world-class jazz ensemble, delivering timeless performances filled with intricate improvisations, swinging rhythms, and soulful melodies. Inspired by the legends of bebop, swing, and fusion, they keep the spirit of jazz alive with every note.", ImageUrl = "jazzmaster.jpg" }, //5
                new Artist { UserId = 13, Name = "Always Punks", About = "A high-octane punk band straight from London, serving up fast-paced riffs, rebellious anthems, and raw, unapologetic energy. With influences from classic UK punk and modern hardcore, they embody the spirit of DIY rebellion and underground chaos.", ImageUrl = "alwayspunks.jpg" }, //6
                new Artist { UserId = 14, Name = "The Hollow Frequencies", About = "A mysterious and atmospheric rock/indie band blending shoegaze textures, post-rock soundscapes, and eerie electronic elements. Their music creates a dreamlike experience, full of ethereal vocals, reverb-heavy guitars, and hypnotic beats. Fans of Radiohead, Slowdive, and Tame Impala will appreciate their unique sonic depth.", ImageUrl = "hollowfrequencies.jpg" }, //7
                new Artist { UserId = 15, Name = "Neon Foxes", About = "An electrifying indie rock outfit with a retro-futuristic twist. Known for their bright neon aesthetics, punchy guitar riffs, and synth-laden hooks, they seamlessly fuse new wave, alternative rock, and synth-pop. Imagine The Killers meets CHVRCHES, with a dash of 80s nostalgia.", ImageUrl = "neonfoxes.jpg" }, //8
                new Artist { UserId = 16, Name = "Velvet Static", About = "An alternative rock band with a grungy, raw energy and deep emotional lyricism. They mix 90s grunge, modern alt-rock, and electronic noise elements, creating a heavy yet melancholic sound. Think Nine Inch Nails meets Wolf Alice, wrapped in a haze of distortion and emotion.", ImageUrl = "velvetstatic.jpg" }, //9
                new Artist { UserId = 17, Name = "Echo Bloom", About = "A delicate fusion of indie folk, dream pop, and post-rock, Echo Bloom crafts beautifully cinematic, reverb-soaked melodies. Their sound is gentle yet expansive, full of lush harmonies and introspective lyrics. Perfect for fans of Fleet Foxes, Daughter, and Sigur Rós.", ImageUrl = "echobloom.jpg" }, //10
                new Artist { UserId = 18, Name = "The Wild Chords", About = "A high-energy rock band with classic rock roots and a punk-inspired edge. Their music is full of blazing guitar solos, anthemic choruses, and rebellious energy, echoing the sounds of Foo Fighters, The White Stripes, and The Black Keys. Pure stadium rock energy meets garage rock grit.", ImageUrl = "wildchords.jpg" }, //11
                new Artist { UserId = 19, Name = "Glitch & Glow", About = "A cutting-edge electropop duo that merges glitchy beats, shimmering synths, and futuristic melodies. Their music is playful yet deeply layered, drawing from hyperpop, synthwave, and experimental electronica. Think Grimes meets 100 gecs with a neon cyberpunk glow.", ImageUrl = "glitchandglow.jpg" }, //12
                new Artist { UserId = 20, Name = "Sonic Mirage", About = "A boundary-pushing artist blending ambient pop, chillwave, and experimental electronica. With dreamy vocal manipulations, warped synth textures, and hypnotic beats, their music transports listeners into an otherworldly sonic realm. Fans of James Blake, FKA twigs, and Bon Iver’s electronic work will feel at home.", ImageUrl = "sonicmirage.jpg" }, //13
                new Artist { UserId = 21, Name = "Neon Echoes", About = "An infectious pop-electronic project with a penchant for shiny hooks, bouncy basslines, and nostalgic 80s synth tones. They craft high-energy anthems perfect for late-night city drives and festival dancefloors. Think Dua Lipa meets The Weeknd’s After Hours era.", ImageUrl = "neonechoes.jpg" }, //14
                new Artist { UserId = 22, Name = "Dreamwave Collective", About = "A synthwave-inspired collective that blends retro-futuristic aesthetics with modern dance music. Their music is full of lush pads, pulsating basslines, and dreamy vocals, creating a nostalgic yet fresh soundscape. Ideal for fans of Tycho, M83, and Kavinsky.", ImageUrl = "dreamwavecollective.jpg" }, //15
                new Artist { UserId = 23, Name = "Synth Pulse", About = "A high-energy electro-house act that thrives on pounding beats, pulsing synth rhythms, and euphoric drops. Their music is engineered for massive club nights and festival main stages, blending influences from Daft Punk, Justice, and Deadmau5.", ImageUrl = "synthpulse.jpg" }, //16
                new Artist { UserId = 24, Name = "The Brass Poets", About = "A modern jazz-hip-hop fusion group, blending slick brass arrangements, spoken-word poetry, and jazzy boom-bap beats. Their music is both sophisticated and raw, reminiscent of Robert Glasper meets A Tribe Called Quest.", ImageUrl = "brasspoets.jpg" }, //17
                new Artist { UserId = 25, Name = "Groove Alchemy", About = "A genre-blending hip-hop, funk, and jazz ensemble known for their infectious grooves, soulful samples, and dynamic live instrumentation. They mix classic jazz with hip-hop storytelling, creating something that feels both vintage and contemporary. Fans of Anderson .Paak, J Dilla, and The Roots will love their sound.", ImageUrl = "groovealchemy.jpg" }, //18
                new Artist { UserId = 26, Name = "Velvet Rhymes", About = "A smooth and soulful hip-hop act incorporating laid-back jazz vibes, silky R&B vocals, and introspective lyricism. Their sound is moody and intimate, perfect for late-night contemplation. Think Common meets D’Angelo with a touch of lo-fi jazz-hop.", ImageUrl = "velvetrhymes.jpg" }, //19
                new Artist { UserId = 27, Name = "The Lo-Fi Syndicate", About = "A collective of producers, beatmakers, and instrumentalists crafting chilled-out, atmospheric lo-fi beats. Their sound is perfect for study sessions, rainy days, and meditative relaxation. They pull influences from J Dilla, Nujabes, and Flying Lotus.", ImageUrl = "lofisyndicate.jpg" }, //20
                new Artist { UserId = 28, Name = "Beats & Blue Notes", About = "A vibrant jazz-hip-hop crossover act, weaving swing-inspired horn sections, turntablism, and laid-back rap flows. They blend classic bebop energy with modern hip-hop rhythms, appealing to fans of Guru’s Jazzmatazz and Madlib’s Blue Note remixes.", ImageUrl = "beatsbluenotes.jpg" }, //21
                new Artist { UserId = 29, Name = "Bass Pilots", About = "A high-octane drum & bass DJ/producer duo delivering fast-paced, high-energy bass drops, intricate breakbeats, and futuristic soundscapes. Their sets keep the crowd moving non-stop. Think Chase & Status meets Noisia.", ImageUrl = "basspilots.jpg" }, //22
                new Artist { UserId = 30, Name = "The Digital Prophets", About = "A collective at the cutting edge of AI-infused electronic music, merging techno, house, and glitchy IDM elements. Their music feels like a prophecy of the future of club sound, drawing from Aphex Twin, Four Tet, and Richie Hawtin.", ImageUrl = "digitalprophets.jpg" }, //23
                new Artist { UserId = 31, Name = "Neon Bass Theory", About = "A futuristic bass music act, fusing deep dubstep, DnB, and cyberpunk aesthetics. Their music feels like stepping into a neon-lit sci-fi rave, with thick sub-bass and glitchy, mechanical beats. Think Rezz meets The Prodigy.", ImageUrl = "neonbasstheory.jpg" }, //24
                new Artist { UserId = 32, Name = "Wavelength 303", About = "A house/techno producer inspired by classic acid house and Detroit techno. With hypnotic 303 basslines, pulsating four-on-the-floor rhythms, and atmospheric textures, their sound pays homage to pioneers like Carl Cox, Daft Punk, and The Chemical Brothers.", ImageUrl = "wavelength303.jpg" }, //25
                new Artist { UserId = 33, Name = "Gravity Loops", About = "A deep house and future garage project that thrives on soulful vocal chops, atmospheric synth layers, and rhythmic house grooves. Their sound is perfect for sunset beach parties and underground club nights. Think Disclosure meets Burial.", ImageUrl = "gravityloops.jpg" }, //26
                new Artist { UserId = 34, Name = "The Golden Reverie", About = "A genre-fluid rock/pop collective that blends grand orchestral arrangements with stadium-sized indie anthems. Their sound is lush, cinematic, and emotionally powerful, akin to Coldplay, Arcade Fire, and Florence + The Machine.", ImageUrl = "goldenreverie.jpg" }, //27
                new Artist { UserId = 35, Name = "Fable Sound", About = "A mythical and storytelling-driven alt-rock band known for their fantastical lyricism, epic song structures, and progressive influences. Their music is an adventure, drawing comparisons to Muse, Coheed and Cambria, and Of Monsters and Men.", ImageUrl = "fablesound.jpg" }, //28
                new Artist { UserId = 36, Name = "Moonlight Static", About = "A lo-fi indie-pop band with a dreamy, melancholic touch. Their songs are nostalgic and introspective, with soft vocals, ambient synths, and laid-back guitar tones. Think Beach House, Cigarettes After Sex, and The xx.", ImageUrl = "moonlightstatic.jpg" }, //29
                new Artist { UserId = 37, Name = "The Chromatics", About = "A genre-blending synth-rock band that fuses post-punk, indie-pop, and 80s electronic influences. Their sound is both retro and modern, perfect for fans of The Cure, New Order, and Chromatics (the actual band!).", ImageUrl = "thechromatics.jpg" }, //30
                new Artist { UserId = 38, Name = "Echo Reverberation", About = "A band that thrives on psychedelic indie vibes, experimental production, and reverb-drenched guitars. Their music is hypnotic, hallucinatory, and atmospheric—think Tame Impala, My Bloody Valentine, and MGMT.", ImageUrl = "echoreverberation.jpg" }, //31
                new Artist { UserId = 39, Name = "Midnight Reverie", About = "A dreamy yet electrifying alt-rock band that blends atmospheric synth textures with soaring guitar riffs and emotionally charged vocals. Their sound moves effortlessly between melancholic ballads and euphoric anthems, creating a cinematic experience. Inspired by The War on Drugs, Wolf Alice, and The Killers, they thrive on nostalgia-laced melodies and expansive, reverb-drenched soundscapes. Their music is perfect for late-night drives and introspective moments.", ImageUrl = "midnightreverie.jpg" }, //32
                new Artist { UserId = 40, Name = "Static Wolves", About = "A gritty, high-energy rock band that combines garage rock rawness with alternative rock’s polished intensity. Their songs feature raspy, anthemic vocals, punchy drum patterns, and distorted, riff-heavy guitars that cut through the noise like a wild animal in the night. Fans of Royal Blood, Arctic Monkeys, and The White Stripes will love their swagger-filled, rebellious energy that feels both dangerous and electrifying.", ImageUrl = "staticwolves.jpg" }, //33
                new Artist { UserId = 41, Name = "Echo Collapse", About = "A post-punk revival band with a cinematic and introspective edge, Echo Collapse thrives on moody basslines, haunting vocals, and hypnotic drum patterns. Their sound is both melancholic and powerful, pulling inspiration from Interpol, The Cure, and Joy Division. With a dark yet melodic approach, they create music that resonates with outsiders, night wanderers, and those lost in thought.", ImageUrl = "echocollapse.jpg" }, //34
                new Artist { UserId = 42, Name = "Violet Sundown", About = "A psychedelic indie band that fuses alternative rock, dream pop, and shoegaze influences, creating a kaleidoscope of lush soundscapes. Their music is characterized by swirling guitars, hazy vocals, and hypnotic rhythms, pulling listeners into a trance-like state. Think Tame Impala meets Beach House, with a touch of My Bloody Valentine. Their sound is ethereal yet grounded, nostalgic yet futuristic, making them a favorite for deep thinkers and cosmic dreamers.", ImageUrl = "violetsundown.jpg" } //35

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
                new Venue { UserId = 43, Name = "The Grand Venue", About = "Tucked away in the heart of Leatherhead, The Grand Venue has long been a cornerstone of the town’s cultural scene. Originally built as a Victorian-era community hall, it later transformed into an intimate venue for folk nights, jazz performances, and local theatre productions. With its wooden beam ceiling, vintage chandeliers, and a snug bar serving craft ales, it exudes an old-world charm. Today, it remains a go-to spot for grassroots artists and hosts everything from acoustic showcases to spoken word nights.", ImageUrl = "grandvenue.jpg", Approved = true }, //1
new Venue { UserId = 44, Name = "Redhill Hall", About = "Redhill Hall is a historic building that has served as a gathering place for musicians, poets, and artists since the late 1800s. Originally a town assembly hall, it was repurposed into a performance space in the 1970s, providing an intimate stage for indie bands, classical quartets, and local theatre productions. With its red-bricked exterior, arched windows, and candle-lit interior, the hall is both nostalgic and atmospheric—a hidden gem for folk nights and unplugged performances.", ImageUrl = "redhillhall.jpg", Approved = true }, //2
new Venue { UserId = 45, Name = "Weybridge Pavilion", About = "Originally a community center built in the 1950s, Weybridge Pavilion has become a beloved venue for up-and-coming indie bands and alternative rock groups. Its modest stage and open floor layout allow for intimate yet energetic performances, often bringing in local talent and traveling artists. On weekends, it doubles as a DIY art space, showcasing photography exhibitions and spoken-word events. Locals love it for its laid-back atmosphere, low lighting, and vintage posters lining the walls.", ImageUrl = "weybridgepavilon.jpg", Approved = true }, //3
new Venue { UserId = 46, Name = "Cobham Arts Centre", About = "The Cobham Arts Centre was founded in the early 1990s by a group of artists who wanted to create a dedicated space for music, theatre, and visual arts. Built in a converted warehouse, it retains its industrial charm, with exposed brick walls, large arched windows, and a multipurpose stage that accommodates everything from classical recitals to electronic music nights. It’s a favorite among experimental musicians and alternative theatre groups, attracting a crowd that appreciates art in all forms.", ImageUrl = "cobhamarts.jpg", Approved = true }, //4
new Venue { UserId = 47, Name = "Chertsey Arena", About = "Unlike most small venues, Chertsey Arena was purpose-built in the 1980s as a regional music and performance venue. While it can hold larger crowds, it maintains a tight-knit community feel, regularly hosting tribute acts, battle of the bands, and grassroots punk shows. The venue is known for its dim neon lights, dark wood bar, and posters from past decades covering every inch of the walls.", ImageUrl = "chertseyarena.jpg", Approved = true }, //5
new Venue { UserId = 48, Name = "Camden Electric Ballroom", About = "A legendary venue in London’s Camden Town, the Electric Ballroom has been a fixture of the alternative music scene since the 1950s. Originally a dance hall, it later became an iconic spot for punk, rock, and electronic gigs, with bands like The Clash and The Smiths once gracing its stage. Today, the venue retains its underground charm, with a graffiti-covered exterior, a packed standing area, and a history steeped in counterculture.", ImageUrl = "camdenballroom.jpg", Approved = true }, //6
new Venue { UserId = 49, Name = "Manchester Night & Day Café", About = "A staple of Manchester’s Northern Quarter, Night & Day Café has been at the heart of the city’s indie music scene since the 1990s. Known for launching the careers of Britpop and post-punk revival bands, this cozy, café-style venue offers a laid-back atmosphere by day and electric energy by night. With wood-paneled walls, vintage gig posters, and low-hanging bulbs illuminating the stage, it’s the perfect spot for emerging bands and intimate acoustic nights.", ImageUrl = "manchesternightday.jpg", Approved = true }, //7
new Venue { UserId = 50, Name = "Birmingham O2 Institute", About = "Housed in a former church, Birmingham’s O2 Institute is a stunning mix of gothic architecture and modern music culture. Built in the early 1900s, its arched ceilings and stained-glass windows provide a striking contrast to the rock, indie, and electronic acts that now take the stage. It’s a favorite among touring artists who love its intimate yet grand feel.", ImageUrl = "birminghamo2.jpg", Approved = true }, //8
new Venue { UserId = 51, Name = "Edinburgh Usher Hall", About = "One of Scotland’s most renowned venues, Usher Hall has been a premier location for classical concerts, jazz performances, and contemporary acts since the early 20th century. The venue’s ornate architecture, velvet drapes, and pristine acoustics make it a sought-after stage for musicians of all genres. While it leans towards orchestral and folk performances, it has also welcomed alternative rock and indie musicians over the years.", ImageUrl = "edinburghusher.jpg", Approved = true }, //9
new Venue { UserId = 52, Name = "Liverpool Philharmonic Hall", About = "A jewel of Liverpool’s music scene, the Philharmonic Hall was built in 1939 and is home to the Royal Liverpool Philharmonic Orchestra. Known for its exceptional acoustics, it attracts jazz musicians, orchestras, and even intimate rock performances. The venue’s classic Art Deco design, luxurious seating, and historic charm make it one of the most treasured cultural spaces in the city.", ImageUrl = "liverpoolphilharmonic.jpg", Approved = true }, //10
new Venue { UserId = 53, Name = "Leeds Brudenell Social Club", About = "A legendary indie music venue that has remained authentically grassroots since its founding in 1913 as a working men’s club. Over the years, it evolved into a haven for DIY musicians, alternative rock bands, and underground artists. The club retains its community feel, with a no-frills bar, simple wooden seating, and a tiny, sweat-soaked stage where future stars are born. It’s the kind of place where intimacy and raw energy define the experience.", ImageUrl = "leedsbrudenell.jpg", Approved = true }, //11
new Venue { UserId = 54, Name = "Glasgow Barrowland Ballroom", About = "A historic music hall dating back to 1934, the Barrowland Ballroom is a Glaswegian institution. Once a dance hall for swing and jazz lovers, it now hosts some of the biggest indie and rock acts, yet still feels deeply connected to its blue-collar roots. With its retro neon sign, vintage Art Deco interior, and a bouncing wooden floor that vibrates with the crowd, it’s one of the most beloved live music venues in Scotland.", ImageUrl = "glasgowbarrowland.jpg", Approved = true }, //12
new Venue { UserId = 55, Name = "Sheffield Leadmill", About = "Opening its doors in 1980, The Leadmill is Sheffield’s oldest live music venue and a launchpad for alternative bands, punk groups, and indie rockers. It has played host to early performances from Pulp, Arctic Monkeys, and The Killers, and its low ceilings, intimate stage, and sticky floors make it a true dive bar venue where every gig feels like a secret show. It’s loud, gritty, and full of character.", ImageUrl = "sheffieldleadmill.jpg", Approved = true }, //13
new Venue { UserId = 56, Name = "Nottingham Rock City", About = "Since its opening in 1980, Rock City has earned a reputation as one of the UK’s most iconic rock venues. Hosting everything from metal to alternative rock, its graffiti-covered walls, booming sound system, and multi-room layout make it a mecca for headbangers and mosh pits. Every weekend, up-and-coming punk bands share the stage with established acts, ensuring that the spirit of rock stays alive in Nottingham.", ImageUrl = "nottinghamrockcity.jpg", Approved = true }, //14
new Venue { UserId = 57, Name = "Bristol Thekla", About = "Possibly the UK’s most unique music venue, Thekla is a repurposed cargo ship that has floated in Bristol Harbour since 1984. The venue’s industrial metal interiors, low ceilings, and multi-level standing areas create a truly immersive experience. Known for its alternative club nights, drum & bass sets, and indie band showcases, it attracts music lovers who crave something out of the ordinary.", ImageUrl = "bristolthekla.jpg", Approved = true }, //15
new Venue { UserId = 58, Name = "Brighton Concorde 2", About = "Sitting on Brighton Beach, Concorde 2 is a seaside club with a reputation for legendary electronic music nights. Originally a Victorian tea room, it was transformed into a club venue in the 1990s, hosting house DJs, drum & bass artists, and live indie acts. With its arched windows looking out onto the ocean, a minimalist dance floor, and a powerful sound system, it’s a favorite for those who love coastal nightlife with a bit of history.", ImageUrl = "brightonconcorde2.jpg", Approved = true }, //16
new Venue { UserId = 59, Name = "Cardiff Tramshed", About = "A former tram depot, the Tramshed is now one of Cardiff’s most dynamic music and arts venues. With its brick walls, warehouse-style open space, and high industrial ceilings, it caters to indie, hip-hop, and electronic artists alike. A favorite for both emerging and established acts, it combines urban grit with a creative artsy feel, making every gig feel raw and spontaneous.", ImageUrl = "cardifftramshed.jpg", Approved = true }, //17
new Venue { UserId = 60, Name = "Newcastle O2 Academy", About = "Originally a cinema built in 1927, the Newcastle O2 Academy still carries the grandeur of its past. Though it has been converted into a live music venue, it retains its ornate ceilings, sloped viewing area, and a grand yet intimate atmosphere. Hosting everything from rock gigs to hip-hop nights, it’s a staple of Newcastle’s music scene, where every show feels like a special event.", ImageUrl = "newcastleo2.jpg", Approved = true }, //18
new Venue { UserId = 61, Name = "Oxford O2 Academy", About = "Tucked into the heart of Oxford, the O2 Academy has been a hotspot for indie and alternative music since the early 2000s. The venue’s no-frills design, intimate layout, and pulsating energy make it a favorite for student crowds and local music lovers. Whether it’s a high-energy rock show or an intimate acoustic gig, the Academy delivers pure live music energy.", ImageUrl = "oxfordo2.jpg", Approved = true }, //19
new Venue { UserId = 62, Name = "Cambridge Corn Exchange", About = "Built in 1875, the Corn Exchange was once a trading hub for local merchants. Today, it’s a thriving concert hall known for its versatile performances—from classical concerts to indie rock gigs. The venue’s high ceilings, grand interior, and historic brickwork give it a regal feel, yet it maintains a cozy atmosphere that makes every show feel special.", ImageUrl = "cambridgecornexchange.jpg", Approved = true }, //20
new Venue { UserId = 63, Name = "Bath Komedia", About = "Originally a cinema in the 1920s, Komedia is now one of Bath’s most beloved arts venues, hosting live music, stand-up comedy, and indie performances. Its retro Art Deco aesthetic, dim red lighting, and intimate tables create a cabaret-style setting, perfect for a laid-back yet lively night out.", ImageUrl = "bathkomedia.jpg", Approved = true }, //21
new Venue { UserId = 64, Name = "Aberdeen The Lemon Tree", About = "Nestled in Aberdeen’s city center, The Lemon Tree has been a cultural hub for arts and music since the 1990s. Originally a warehouse space, it was converted into a small live venue that quickly became a favorite for indie bands, folk musicians, and spoken word artists. With its low ceilings, exposed brick walls, and intimate candlelit tables, it’s a venue where audiences are up close and personal with the performers.", ImageUrl = "aberdeenlemontree.jpg", Approved = true }, //22
new Venue { UserId = 65, Name = "York Barbican", About = "First opening in 1991, the York Barbican was built as a multi-purpose events hall. While it primarily hosts comedy, jazz, and classical performances, it also brings in rock, indie, and folk acts. Its modern yet intimate design—with a sloped viewing area, black-painted interiors, and simple stage lighting—makes it an inviting venue for both seated and standing audiences.", ImageUrl = "yorkbarbican.jpg", Approved = true }, //23
new Venue { UserId = 66, Name = "Belfast Limelight", About = "A true staple of Belfast’s alternative music scene, the Limelight has been pumping out rock, punk, and indie gigs since the 1980s. The venue consists of multiple rooms, including a smaller, grungy dive bar stage and a slightly larger gig room with low ceilings, neon bar signs, and black-painted walls. It’s the kind of place where up-and-coming bands cut their teeth before making it big.", ImageUrl = "belfastlimelight.jpg", Approved = true }, //24
new Venue { UserId = 67, Name = "Dublin Vicar Street", About = "Since opening in 1998, Vicar Street has built a reputation as Dublin’s most beloved live performance space. It’s known for hosting intimate gigs with world-famous artists, thanks to its cozy yet elegant layout. With wood-paneled walls, dim hanging lights, and a large but intimate standing area, it’s a venue that feels high-class yet unpretentious, making it the perfect stage for indie rock bands, singer-songwriters, and jazz ensembles.", ImageUrl = "dublinvicarstreet.jpg", Approved = true }, //25
new Venue { UserId = 68, Name = "Norwich Waterfront", About = "Once a warehouse in the city’s docklands, the Waterfront became a music venue in the early 1990s, quickly growing into Norwich’s go-to spot for indie, punk, and alternative rock. The venue is dark and atmospheric, with a balcony area overlooking the stage, a black-painted ceiling, and gig posters covering the walls. The raw industrial feel gives it underground credibility, making it one of the best intimate venues in the UK.", ImageUrl = "norwichwaterfront.jpg", Approved = true }, //26
new Venue { UserId = 69, Name = "Exeter Phoenix", About = "Originally an arts and community center, the Phoenix is now Exeter’s leading multi-arts venue, known for its experimental theatre, visual art installations, and intimate music performances. The venue maintains its indie charm, with a small wooden stage, fairy lights hanging from the ceiling, and colorful murals decorating the walls. It’s the perfect home for emerging folk musicians, indie artists, and spoken word performers.", ImageUrl = "exeterphoenix.jpg", Approved = true }, //27
new Venue { UserId = 70, Name = "Southampton Engine Rooms", About = "A converted industrial unit, the Engine Rooms is one of Southampton’s leading live music spaces. With its concrete floors, neon strip lighting, and a large bar area, it has an urban warehouse feel that makes it ideal for electronic music nights, indie gigs, and alternative club events. The space is simple but effective—no fancy seating, just standing room and a powerful sound system.", ImageUrl = "southamptonengine.jpg", Approved = true }, //28
new Venue { UserId = 71, Name = "Hull The Welly Club", About = "The Welly Club has been Hull’s favorite indie and rock venue since the 1980s. Its quirky name comes from its original life as a social club for working-class locals before it became a hotspot for Britpop bands and underground punk acts. It’s gritty, low-lit, and filled with a mix of dedicated gig-goers and students looking for a great night out.", ImageUrl = "hullwellyclub.jpg", Approved = true }, //29
new Venue { UserId = 72, Name = "Plymouth Junction", About = "Once an old railway building, the Junction became Plymouth’s leading grassroots music venue in the early 2000s. Specializing in punk, metal, and alternative gigs, it has an underground, rebellious feel, with steel beams exposed, faded posters from past gigs, and a DIY stage that looks thrown together but delivers big energy.", ImageUrl = "plymouthjunction.jpg", Approved = true }, //30
new Venue { UserId = 73, Name = "Swansea Sin City", About = "A small but rowdy venue, Sin City is Swansea’s go-to spot for indie, rock, and alternative electronic music. With graffiti-covered walls, LED lighting, and a standing room-only layout, it delivers a chaotic but thrilling gig experience. The venue is infamous for sweaty mosh pits, wild DJ sets, and some of the best up-and-coming bands on tour.", ImageUrl = "swanseasincity.jpg", Approved = true }, //31
new Venue { UserId = 74, Name = "Inverness Ironworks", About = "The Ironworks is one of the Highlands' most important music venues, offering a rare space for touring bands in Scotland’s north. Built in an old industrial building, it blends rugged charm with professional staging, making it one of the most versatile venues in the country. Whether it’s a metal gig, a folk night, or a high-energy ceilidh, it provides a home for all genres and audiences.", ImageUrl = "invernessironworks.jpg", Approved = true }, //32
new Venue { UserId = 75, Name = "Stirling Albert Halls", About = "Dating back to the 1800s, the Albert Halls in Stirling originally hosted town meetings, orchestral performances, and dance nights. Now, it’s a stunning heritage venue used for folk music, jazz concerts, and intimate classical recitals. Its stained-glass windows, velvet-draped stage, and elegant chandeliers make it feel almost like a mini opera house, adding a touch of grandeur to every performance.", ImageUrl = "stirlingalberthalls.jpg", Approved = true }, //33
new Venue { UserId = 76, Name = "Dundee Fat Sams", About = "A legendary rock club, Fat Sams has been Dundee’s number one live venue since the 1980s. It has an underground club feel, with a graffiti-covered entrance, blacked-out walls, and a huge dance floor where people cram together for the most raucous rock gigs. A favorite for late-night alternative music lovers, it’s a venue that never seems to sleep.", ImageUrl = "dundeefatsams.jpg", Approved = true }, //34
new Venue { UserId = 77, Name = "Coventry Empire", About = "Live music and club nights", ImageUrl = "coventryempire.jpg", Approved = true } //35


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

                                    // ListingId 14
                    new ListingApplication { ArtistId = 1, ListingId = 14 }, //33
                    new ListingApplication { ArtistId = 2, ListingId = 14 }, //34
                    new ListingApplication { ArtistId = 3, ListingId = 14 }, //35
                    new ListingApplication { ArtistId = 4, ListingId = 14 }, //36

                    // ListingId 15
                    new ListingApplication { ArtistId = 5, ListingId = 15 }, //37
                    new ListingApplication { ArtistId = 6, ListingId = 15 }, //38
                    new ListingApplication { ArtistId = 7, ListingId = 15 }, //39
                    new ListingApplication { ArtistId = 8, ListingId = 15 }, //40

                    // ListingId 16
                    new ListingApplication { ArtistId = 9, ListingId = 16 }, //41
                    new ListingApplication { ArtistId = 10, ListingId = 16 }, //42
                    new ListingApplication { ArtistId = 11, ListingId = 16 }, //43
                    new ListingApplication { ArtistId = 12, ListingId = 16 }, //44

                    // ListingId 17
                    new ListingApplication { ArtistId = 13, ListingId = 17 }, //45
                    new ListingApplication { ArtistId = 14, ListingId = 17 }, //46
                    new ListingApplication { ArtistId = 15, ListingId = 17 }, //47
                    new ListingApplication { ArtistId = 16, ListingId = 17 }, //48

                    // ListingId 18
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
                        new Event { ApplicationId = 1, Name = "The Rockers performing at The Grand Venue", About = "An epic night of rock music at The Grand Venue.", Price = 15, TotalTickets = 120, AvailableTickets = 80, DatePosted = now.AddDays(-58) }, //1
                        new Event { ApplicationId = 2, Name = "Indie Vibes performing at Redhill Hall", About = "A cozy indie evening at Redhill Hall.", Price = 12, TotalTickets = 110, AvailableTickets = 70, DatePosted = now.AddDays(-55) }, //2
                        new Event { ApplicationId = 3, Name = "Electronic Pulse performing at Weybridge Pavilion", About = "Electronic Pulse takes over Weybridge Pavilion.", Price = 18, TotalTickets = 130, AvailableTickets = 100, DatePosted = now.AddDays(-52) }, //3
                        new Event { ApplicationId = 4, Name = "Hip-Hop Flow performing at Cobham Arts Centre", About = "Experience raw beats at Cobham Arts Centre.", Price = 10, TotalTickets = 100, AvailableTickets = 60, DatePosted = now.AddDays(-49) }, //4
                        new Event { ApplicationId = 5, Name = "Jazz Masters performing at Chertsey Arena", About = "Smooth jazz vibes in Chertsey Arena.", Price = 25, TotalTickets = 140, AvailableTickets = 110, DatePosted = now.AddDays(-46) }, //5
                        new Event { ApplicationId = 6, Name = "Always Punks performing at Camden Electric Ballroom", About = "High-energy punk show in Camden.", Price = 20, TotalTickets = 150, AvailableTickets = 90, DatePosted = now.AddDays(-43) }, //6
                        new Event { ApplicationId = 7, Name = "The Hollow Frequencies performing at Manchester Night & Day Café", About = "Dreamy atmospheres meet indie rock.", Price = 30, TotalTickets = 170, AvailableTickets = 150, DatePosted = now.AddDays(-40) }, //7
                        new Event { ApplicationId = 8, Name = "Neon Foxes performing at Birmingham O2 Institute", About = "Neon lights and synth-rock beats.", Price = 16, TotalTickets = 130, AvailableTickets = 100, DatePosted = now.AddDays(-37) }, //8
                        new Event { ApplicationId = 9, Name = "Velvet Static performing at Edinburgh Usher Hall", About = "A blend of grunge and ambience.", Price = 14, TotalTickets = 115, AvailableTickets = 75, DatePosted = now.AddDays(-34) }, //9
                        new Event { ApplicationId = 10, Name = "Echo Bloom performing at Liverpool Philharmonic Hall", About = "Cinematic folk and lush harmonies.", Price = 22, TotalTickets = 135, AvailableTickets = 100, DatePosted = now.AddDays(-31) }, //10
                        new Event { ApplicationId = 11, Name = "The Wild Chords performing at Leeds Brudenell Social Club", About = "Garage rock energy unleashed.", Price = 13, TotalTickets = 125, AvailableTickets = 85, DatePosted = now.AddDays(-28) }, //11
                        new Event { ApplicationId = 12, Name = "Glitch & Glow performing at Glasgow Barrowland Ballroom", About = "Experimental beats with neon twists.", Price = 11, TotalTickets = 120, AvailableTickets = 90, DatePosted = now.AddDays(-25) }, //12
                        new Event { ApplicationId = 13, Name = "Sonic Mirage performing at Sheffield Leadmill", About = "A journey through ambient pop.", Price = 19, TotalTickets = 140, AvailableTickets = 110, DatePosted = now.AddDays(-22) }, //13
                        new Event { ApplicationId = 14, Name = "Neon Echoes performing at Nottingham Rock City", About = "Retro synths light up the night.", Price = 17, TotalTickets = 135, AvailableTickets = 105, DatePosted = now.AddDays(-19) }, //14
                        new Event { ApplicationId = 15, Name = "Dreamwave Collective performing at Bristol Thekla", About = "Waves of sound aboard Thekla.", Price = 21, TotalTickets = 145, AvailableTickets = 115, DatePosted = now.AddDays(-16) }, //15
                        new Event { ApplicationId = 16, Name = "Synth Pulse performing at Brighton Concorde 2", About = "Massive drops and electronic rush.", Price = 18, TotalTickets = 140, AvailableTickets = 120, DatePosted = now.AddDays(-13) }, //16
                        new Event { ApplicationId = 17, Name = "The Brass Poets performing at Cardiff Tramshed", About = "Brass and poetry unite in Cardiff.", Price = 26, TotalTickets = 155, AvailableTickets = 130, DatePosted = now.AddDays(-10) }, //17
                        new Event { ApplicationId = 18, Name = "Groove Alchemy performing at Newcastle O2 Academy", About = "Funky grooves fill Newcastle.", Price = 15, TotalTickets = 120, AvailableTickets = 100, DatePosted = now.AddDays(-7) }, //18
                        new Event { ApplicationId = 19, Name = "Velvet Rhymes performing at Oxford O2 Academy", About = "Hip-hop with a smooth jazz core.", Price = 28, TotalTickets = 160, AvailableTickets = 145, DatePosted = now.AddDays(-4) }, //19
                        new Event { ApplicationId = 20, Name = "The Lo-Fi Syndicate performing at Cambridge Corn Exchange", About = "Relaxed beats and lo-fi chill.", Price = 24, TotalTickets = 150, AvailableTickets = 130, DatePosted = now.AddDays(-1) }, //20
                        new Event { ApplicationId = 21, Name = "Beats & Blue Notes performing at Bath Komedia", About = "Bebop swing meets rhymes.", Price = 27, TotalTickets = 160, AvailableTickets = 140, DatePosted = now.AddDays(2) }, //21
                        new Event { ApplicationId = 22, Name = "Bass Pilots performing at Aberdeen The Lemon Tree", About = "DnB madness in Aberdeen.", Price = 23, TotalTickets = 130, AvailableTickets = 100, DatePosted = now.AddDays(5) }, //22
                        new Event { ApplicationId = 23, Name = "The Digital Prophets performing at York Barbican", About = "Glitchy, futuristic techno journey.", Price = 29, TotalTickets = 155, AvailableTickets = 140, DatePosted = now.AddDays(8) }, //23
                        new Event { ApplicationId = 24, Name = "Neon Bass Theory performing at Belfast Limelight", About = "Bass-driven neon dreamscapes.", Price = 10, TotalTickets = 110, AvailableTickets = 70, DatePosted = now.AddDays(11) }, //24
                        new Event { ApplicationId = 25, Name = "Wavelength 303 performing at Dublin Vicar Street", About = "Old school acid house revival.", Price = 15, TotalTickets = 125, AvailableTickets = 90, DatePosted = now.AddDays(14) }, //25
                        new Event { ApplicationId = 26, Name = "Gravity Loops performing at Norwich Waterfront", About = "Deep house and chill fusion night.", Price = 30, TotalTickets = 180, AvailableTickets = 170, DatePosted = now.AddDays(17) }, //26
                        new Event { ApplicationId = 35, Name = "The Rockers performing at The Grand Venue", About = "This is an event for Listing 14 with Artist 1", Price = 20, TotalTickets = 100, AvailableTickets = 80, DatePosted = now.AddDays(6) },
                        new Event { ApplicationId = 39, Name = "Indie Vibes performing at Redhill Hall", About = "This is an event for Listing 15 with Artist 5", Price = 25, TotalTickets = 120, AvailableTickets = 100, DatePosted = now.AddDays(12) },
                        new Event { ApplicationId = 42, Name = "Electronic Pulse performing at Weybridge Pavilion", About = "This is an event for Listing 16 with Artist 9", Price = 30, TotalTickets = 140, AvailableTickets = 120, DatePosted = now.AddDays(18) },
                        new Event { ApplicationId = 45, Name = "Hip-Hop Flow performing at Cobham Arts Centre", About = "This is an event for Listing 17 with Artist 13", Price = 15, TotalTickets = 100, AvailableTickets = 80, DatePosted = now.AddDays(22) },
                        new Event { ApplicationId = 52, Name = "Jazz Masters performing at Chertsey Arena", About = "This is an event for Listing 18 with Artist 17", Price = 20, TotalTickets = 150, AvailableTickets = 130, DatePosted = now.AddDays(25) }

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


        // Messages
        if (!context.Messages.Any())
            {
                var messages = new Message[]
                {
                    // Application submission messages (action = "application")
                    new Message
                    {
                        FromUserId = 8,  // artistmanager1@test.com
                        ToUserId = 43,  // venuemanager1@test.com
                        Action = "application",
                        Content = "artistmanager1@test.com has applied to your listing.",
                        SentDate = now.AddDays(-58),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 9,  // artistmanager2@test.com
                        ToUserId = 44,  // venuemanager2@test.com
                        Action = "application",
                        Content = "artistmanager2@test.com has applied to your listing.",
                        SentDate = now.AddDays(-55),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 10,  // artistmanager3@test.com
                        ToUserId = 45,  // venuemanager3@test.com
                        Action = "application",
                        Content = "artistmanager3@test.com has applied to your listing.",
                        SentDate = now.AddDays(-52),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 11,  // artistmanager4@test.com
                        ToUserId = 46,  // venuemanager4@test.com
                        Action = "application",
                        Content = "artistmanager4@test.com has applied to your listing.",
                        SentDate = now.AddDays(-49),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 12,  // artistmanager5@test.com
                        ToUserId = 47,  // venuemanager5@test.com
                        Action = "application",
                        Content = "artistmanager5@test.com has applied to your listing.",
                        SentDate = now.AddDays(-46),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 13,  // artistmanager6@test.com
                        ToUserId = 48,  // venuemanager6@test.com
                        Action = "application",
                        Content = "artistmanager6@test.com has applied to your listing.",
                        SentDate = now.AddDays(-43),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 14,  // artistmanager7@test.com
                        ToUserId = 49,  // venuemanager7@test.com
                        Action = "application",
                        Content = "artistmanager7@test.com has applied to your listing.",
                        SentDate = now.AddDays(-40),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 15,  // artistmanager8@test.com
                        ToUserId = 50,  // venuemanager8@test.com
                        Action = "application",
                        Content = "artistmanager8@test.com has applied to your listing.",
                        SentDate = now.AddDays(-37),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 16,  // artistmanager9@test.com
                        ToUserId = 51,  // venuemanager9@test.com
                        Action = "application",
                        Content = "artistmanager9@test.com has applied to your listing.",
                        SentDate = now.AddDays(-34),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 17,  // artistmanager10@test.com
                        ToUserId = 52,  // venuemanager10@test.com
                        Action = "application",
                        Content = "artistmanager10@test.com has applied to your listing.",
                        SentDate = now.AddDays(-31),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 18,  // artistmanager11@test.com
                        ToUserId = 53,  // venuemanager11@test.com
                        Action = "application",
                        Content = "artistmanager11@test.com has applied to your listing.",
                        SentDate = now.AddDays(-28),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 19,  // artistmanager12@test.com
                        ToUserId = 54,  // venuemanager12@test.com
                        Action = "application",
                        Content = "artistmanager12@test.com has applied to your listing.",
                        SentDate = now.AddDays(-25),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 20,  // artistmanager13@test.com
                        ToUserId = 55,  // venuemanager13@test.com
                        Action = "application",
                        Content = "artistmanager13@test.com has applied to your listing.",
                        SentDate = now.AddDays(-22),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 21,  // artistmanager14@test.com
                        ToUserId = 56,  // venuemanager14@test.com
                        Action = "application",
                        Content = "artistmanager14@test.com has applied to your listing.",
                        SentDate = now.AddDays(-19),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 22,  // artistmanager15@test.com
                        ToUserId = 57,  // venuemanager15@test.com
                        Action = "application",
                        Content = "artistmanager15@test.com has applied to your listing.",
                        SentDate = now.AddDays(-16),
                        Read = false
                    },

                    // Application acceptance messages (action = "event")
                    new Message
                    {
                        FromUserId = 43,  // venuemanager1@test.com
                        ToUserId = 8,  // artistmanager1@test.com
                        Action = "event",
                        Content = "venuemanager1@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-57),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 44,  // venuemanager2@test.com
                        ToUserId = 9,  // artistmanager2@test.com
                        Action = "event",
                        Content = "venuemanager2@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-54),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 45,  // venuemanager3@test.com
                        ToUserId = 10,  // artistmanager3@test.com
                        Action = "event",
                        Content = "venuemanager3@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-51),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 46,  // venuemanager4@test.com
                        ToUserId = 11,  // artistmanager4@test.com
                        Action = "event",
                        Content = "venuemanager4@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-48),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 47,  // venuemanager5@test.com
                        ToUserId = 12,  // artistmanager5@test.com
                        Action = "event",
                        Content = "venuemanager5@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-45),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 48,  // venuemanager6@test.com
                        ToUserId = 13,  // artistmanager6@test.com
                        Action = "event",
                        Content = "venuemanager6@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-42),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 49,  // venuemanager7@test.com
                        ToUserId = 14,  // artistmanager7@test.com
                        Action = "event",
                        Content = "venuemanager7@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-39),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 50,  // venuemanager8@test.com
                        ToUserId = 15,  // artistmanager8@test.com
                        Action = "event",
                        Content = "venuemanager8@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-36),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 51,  // venuemanager9@test.com
                        ToUserId = 16,  // artistmanager9@test.com
                        Action = "event",
                        Content = "venuemanager9@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-33),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 52,  // venuemanager10@test.com
                        ToUserId = 17,  // artistmanager10@test.com
                        Action = "event",
                        Content = "venuemanager10@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-30),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 53,  // venuemanager11@test.com
                        ToUserId = 18,  // artistmanager11@test.com
                        Action = "event",
                        Content = "venuemanager11@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-27),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 54,  // venuemanager12@test.com
                        ToUserId = 19,  // artistmanager12@test.com
                        Action = "event",
                        Content = "venuemanager12@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-24),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 55,  // venuemanager13@test.com
                        ToUserId = 20,  // artistmanager13@test.com
                        Action = "event",
                        Content = "venuemanager13@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-21),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 56,  // venuemanager14@test.com
                        ToUserId = 21,  // artistmanager14@test.com
                        Action = "event",
                        Content = "venuemanager14@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-18),
                        Read = false
                    },
                    new Message
                    {
                        FromUserId = 57,  // venuemanager15@test.com
                        ToUserId = 22,  // artistmanager15@test.com
                        Action = "event",
                        Content = "venuemanager15@test.com has accepted your application. An event has been created.",
                        SentDate = now.AddDays(-15),
                        Read = false
                    }
            };
                context.Messages.AddRange(messages);
                await context.SaveChangesAsync();
            }

            if (!context.Transactions.Any())
            {
                var transactions = new List<Transaction>
                {
                    // Event payouts: VenueManager (Listing.Venue.UserId) → ArtistManager (Artist.UserId)

                    new Transaction { FromUserId = 43, ToUserId = 8, TransactionId = Guid.NewGuid().ToString(), Amount = 150, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-58) },
                    new Transaction { FromUserId = 44, ToUserId = 9, TransactionId = Guid.NewGuid().ToString(), Amount = 200, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-55) },
                    new Transaction { FromUserId = 45, ToUserId = 10, TransactionId = Guid.NewGuid().ToString(), Amount = 180, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-52) },
                    new Transaction { FromUserId = 46, ToUserId = 11, TransactionId = Guid.NewGuid().ToString(), Amount = 175, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-49) },
                    new Transaction { FromUserId = 47, ToUserId = 12, TransactionId = Guid.NewGuid().ToString(), Amount = 160, Type = "event", Status = "Completed", CreatedAt = now.AddDays(-46) },

                    // Ticket purchases: Customer → VenueManager (based on EventId to VenueId mapping)

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
