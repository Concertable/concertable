using Core.Entities;
using Core.Entities.Identity;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // Users
            if (!context.Users.Any())
            {
                var users = new ApplicationUser[]
                {
                    new Admin { UserName = "admin1@test.com", Email = "admin1@test.com", County = "Surrey", Town = "Woking", Latitude = 51.0, Longitude = -0.5 }, //1
                    new Customer { UserName = "customer1@test.com", Email = "customer1@test.com", County = "Surrey", Town = "Guildford", Latitude = 51.25, Longitude = -0.56 }, //2
                    new Customer { UserName = "customer2@test.com", Email = "customer2@test.com", County = "Surrey", Town = "Epsom", Latitude = 51.34, Longitude = -0.27 }, //3
                    new Customer { UserName = "customer3@test.com", Email = "customer3@test.com", County = "London", Town = "Camden", Latitude = 51.53, Longitude = -0.13 }, //4
                    new Customer { UserName = "customer4@test.com", Email = "customer4@test.com", County = "Edinburgh", Town = "Leith", Latitude = 55.98, Longitude =  -3.17 }, //5
                    new Customer { UserName = "customer5@test.com", Email = "customer5@test.com", County = "Sheffield", Town = "Kelham Island", Latitude = 53.39, Longitude = -1.46 }, //6
                    new Customer { UserName = "customer6@test.com", Email = "customer6@test.com", County = "Bath", Town = "Widcombe", Latitude = 51.38, Longitude = -2.36 }, //7
                    // Artist Managers
                    new ArtistManager { UserName = "artistmanager1@test.com", Email = "artistmanager1@test.com", County = "Surrey", Town = "Dorking", Latitude = 51.23, Longitude = -0.33 }, //8
                    new ArtistManager { UserName = "artistmanager2@test.com", Email = "artistmanager2@test.com", County = "Surrey", Town = "Reigate", Latitude = 51.23, Longitude = -0.17 }, //9
                    new ArtistManager { UserName = "artistmanager3@test.com", Email = "artistmanager3@test.com", County = "Surrey", Town = "Farnham", Latitude = 51.21, Longitude = -0.58 }, //10
                    new ArtistManager { UserName = "artistmanager4@test.com", Email = "artistmanager4@test.com", County = "Surrey", Town = "Camberley", Latitude = 51.34, Longitude = -0.73 }, //11
                    new ArtistManager { UserName = "artistmanager5@test.com", Email = "artistmanager5@test.com", County = "Surrey", Town = "Haslemere", Latitude = 51.08, Longitude = -0.74 }, //12
                    new ArtistManager { UserName = "artistmanager6@test.com", Email = "artistmanager1@test.com", County = "London", Town = "Camden", Latitude = 51.53, Longitude = -0.13 }, //13
                    new ArtistManager { UserName = "artistmanager7@test.com", Email = "artistmanager2@test.com", County = "Manchester", Town = "Salford", Latitude = 53.48, Longitude = -2.25 }, //14
                    new ArtistManager { UserName = "artistmanager8@test.com", Email = "artistmanager3@test.com", County = "Birmingham", Town = "Digbeth", Latitude = 52.47, Longitude = -1.88 }, //15
                    new ArtistManager { UserName = "artistmanager9@test.com", Email = "artistmanager4@test.com", County = "Edinburgh", Town = "Leith", Latitude = 55.98, Longitude = -3.17 }, //16
                    new ArtistManager { UserName = "artistmanager10@test.com", Email = "artistmanager5@test.com", County = "Liverpool", Town = "Baltic Triangle", Latitude = 53.39, Longitude = -2.98 }, //17
                    new ArtistManager { UserName = "artistmanager11@test.com", Email = "artistmanager6@test.com", County = "Leeds", Town = "Headingley", Latitude = 53.82, Longitude = -1.58 }, //18
                    new ArtistManager { UserName = "artistmanager12@test.com", Email = "artistmanager7@test.com", County = "Glasgow", Town = "West End", Latitude = 55.87, Longitude = -4.29 }, //19
                    new ArtistManager { UserName = "artistmanager13@test.com", Email = "artistmanager8@test.com", County = "Sheffield", Town = "Kelham Island", Latitude = 53.39, Longitude = -1.46 }, //20
                    new ArtistManager { UserName = "artistmanager14@test.com", Email = "artistmanager9@test.com", County = "Nottingham", Town = "Lace Market", Latitude = 52.95, Longitude = -1.14 }, //21
                    new ArtistManager { UserName = "artistmanager15@test.com", Email = "artistmanager10@test.com", County = "Bristol", Town = "Stokes Croft", Latitude = 51.46, Longitude = -2.59 }, //22
                    new ArtistManager { UserName = "artistmanager16@test.com", Email = "artistmanager11@test.com", County = "Brighton", Town = "Kemptown", Latitude = 50.82, Longitude = -0.13 }, //23
                    new ArtistManager { UserName = "artistmanager17@test.com", Email = "artistmanager12@test.com", County = "Cardiff", Town = "Cathays", Latitude = 51.49, Longitude = -3.17 }, //24
                    new ArtistManager { UserName = "artistmanager18@test.com", Email = "artistmanager13@test.com", County = "Newcastle", Town = "Jesmond", Latitude = 54.99, Longitude = -1.61 }, //25
                    new ArtistManager { UserName = "artistmanager19@test.com", Email = "artistmanager14@test.com", County = "Oxford", Town = "Jericho", Latitude = 51.76, Longitude = -1.26 }, //26
                    new ArtistManager { UserName = "artistmanager20@test.com", Email = "artistmanager15@test.com", County = "Cambridge", Town = "Mill Road", Latitude = 52.19, Longitude = 0.13 }, //27
                    new ArtistManager { UserName = "artistmanager21@test.com", Email = "artistmanager16@test.com", County = "Bath", Town = "Widcombe", Latitude = 51.38, Longitude = -2.36 }, //28
                    new ArtistManager { UserName = "artistmanager22@test.com", Email = "artistmanager17@test.com", County = "Aberdeen", Town = "Old Aberdeen", Latitude = 57.17, Longitude = -2.1 },  //29
                    new ArtistManager { UserName = "artistmanager23@test.com", Email = "artistmanager18@test.com", County = "York", Town = "The Shambles", Latitude = 53.96, Longitude = -1.08 }, //30
                    new ArtistManager { UserName = "artistmanager24@test.com", Email = "artistmanager19@test.com", County = "Belfast", Town = "Cathedral Quarter", Latitude = 54.6, Longitude = -5.93 }, //31
                    new ArtistManager { UserName = "artistmanager25@test.com", Email = "artistmanager20@test.com", County = "Dublin", Town = "Temple Bar", Latitude = 53.34, Longitude = -6.27 }, //32
                    new ArtistManager { UserName = "artistmanager26@test.com", Email = "artistmanager21@test.com", County = "Norwich", Town = "Tombland", Latitude = 52.63, Longitude = 1.3 }, //33
                    new ArtistManager { UserName = "artistmanager27@test.com", Email = "artistmanager22@test.com", County = "Exeter", Town = "St Sidwell's", Latitude = 50.73, Longitude = -3.53 }, //34
                    new ArtistManager { UserName = "artistmanager28@test.com", Email = "artistmanager23@test.com", County = "Southampton", Town = "Ocean Village", Latitude = 50.9, Longitude = -1.4 }, //35
                    new ArtistManager { UserName = "artistmanager29@test.com", Email = "artistmanager24@test.com", County = "Hull", Town = "Old Town", Latitude = 53.74, Longitude = -0.33 }, //36
                    new ArtistManager { UserName = "artistmanager30@test.com", Email = "artistmanager25@test.com", County = "Plymouth", Town = "The Hoe", Latitude = 50.37, Longitude = -4.14 }, //37
                    new ArtistManager { UserName = "artistmanager31@test.com", Email = "artistmanager26@test.com", County = "Swansea", Town = "Uplands", Latitude = 51.62, Longitude = -3.94 }, //38
                    new ArtistManager { UserName = "artistmanager32@test.com", Email = "artistmanager27@test.com", County = "Inverness", Town = "Dalneigh", Latitude = 57.48, Longitude = -4.23 }, //39
                    new ArtistManager { UserName = "artistmanager33@test.com", Email = "artistmanager28@test.com", County = "Stirling", Town = "Bridge of Allan", Latitude = 56.15, Longitude = -3.93 }, //40
                    new ArtistManager { UserName = "artistmanager34@test.com", Email = "artistmanager29@test.com", County = "Dundee", Town = "Broughty Ferry", Latitude = 56.47, Longitude = -2.87 }, //41
                    new ArtistManager { UserName = "artistmanager35@test.com", Email = "artistmanager30@test.com", County = "Coventry", Town = "Earlsdon", Latitude = 52.40, Longitude = -1.52 }, //42
                    // Venue Managers
                    new VenueManager { UserName = "venuemanager1@test.com", Email = "venuemanager1@test.com", County = "Surrey", Town = "Leatherhead", Latitude = 51.3, Longitude = -0.3 }, //43
                    new VenueManager { UserName = "venuemanager2@test.com", Email = "venuemanager2@test.com", County = "Surrey", Town = "Redhill", Latitude = 51.23, Longitude = -0.17 }, //44
                    new VenueManager { UserName = "venuemanager3@test.com", Email = "venuemanager3@test.com", County = "Surrey", Town = "Weybridge", Latitude = 51.38, Longitude = -0.46 }, //45
                    new VenueManager { UserName = "venuemanager4@test.com", Email = "venuemanager4@test.com", County = "Surrey", Town = "Cobham", Latitude = 51.32, Longitude = -0.46 }, //46
                    new VenueManager { UserName = "venuemanager5@test.com", Email = "venuemanager5@test.com", County = "Surrey", Town = "Chertsey", Latitude = 51.39, Longitude = -0.5 }, //47
                    new VenueManager { UserName = "venuemanager6@test.com", Email = "venuemanager1@test.com", County = "London", Town = "Camden", Latitude = 51.53, Longitude = -0.13 }, //48
                    new VenueManager { UserName = "venuemanager7@test.com", Email = "venuemanager2@test.com", County = "Manchester", Town = "Northern Quarter", Latitude = 53.48, Longitude = -2.23 }, //49
                    new VenueManager { UserName = "venuemanager8@test.com", Email = "venuemanager3@test.com", County = "Birmingham", Town = "Jewellery Quarter", Latitude = 52.48, Longitude = -1.91 }, //50
                    new VenueManager { UserName = "venuemanager9@test.com", Email = "venuemanager4@test.com", County = "Edinburgh", Town = "Old Town", Latitude = 55.95, Longitude = -3.19 }, //51
                    new VenueManager { UserName = "venuemanager10@test.com", Email = "venuemanager5@test.com", County = "Liverpool", Town = "Cavern Quarter", Latitude = 53.41, Longitude = -2.99 }, //52
                    new VenueManager { UserName = "venuemanager11@test.com", Email = "venuemanager6@test.com", County = "Leeds", Town = "Call Lane", Latitude = 53.79, Longitude = -1.54 }, //53
                    new VenueManager { UserName = "venuemanager12@test.com", Email = "venuemanager7@test.com", County = "Glasgow", Town = "Merchant City", Latitude = 55.86, Longitude = -4.24 }, //54
                    new VenueManager { UserName = "venuemanager13@test.com", Email = "venuemanager8@test.com", County = "Sheffield", Town = "Ecclesall Road", Latitude = 53.38, Longitude = -1.50 }, //55
                    new VenueManager { UserName = "venuemanager14@test.com", Email = "venuemanager9@test.com", County = "Nottingham", Town = "Hockley", Latitude = 52.95, Longitude = -1.14 }, //56
                    new VenueManager { UserName = "venuemanager15@test.com", Email = "venuemanager10@test.com", County = "Bristol", Town = "Harbourside", Latitude = 51.45, Longitude = -2.60 }, //57
                    new VenueManager { UserName = "venuemanager16@test.com", Email = "venuemanager11@test.com", County = "Brighton", Town = "The Lanes", Latitude = 50.82, Longitude = -0.14 }, //58
                    new VenueManager { UserName = "venuemanager17@test.com", Email = "venuemanager12@test.com", County = "Cardiff", Town = "Riverside", Latitude = 51.48, Longitude = -3.18 }, //59
                    new VenueManager { UserName = "venuemanager18@test.com", Email = "venuemanager13@test.com", County = "Newcastle", Town = "Quayside", Latitude = 54.97, Longitude = -1.60 }, //60
                    new VenueManager { UserName = "venuemanager19@test.com", Email = "venuemanager14@test.com", County = "Oxford", Town = "Cowley", Latitude = 51.73, Longitude = -1.22 }, //61
                    new VenueManager { UserName = "venuemanager20@test.com", Email = "venuemanager15@test.com", County = "Cambridge", Town = "Chesterton", Latitude = 52.22, Longitude = 0.14 }, //62
                    new VenueManager { UserName = "venuemanager21@test.com", Email = "venuemanager16@test.com", County = "Bath", Town = "Bear Flat", Latitude = 51.37, Longitude = -2.36 }, //63
                    new VenueManager { UserName = "venuemanager22@test.com", Email = "venuemanager17@test.com", County = "Aberdeen", Town = "Footdee", Latitude = 57.15, Longitude = -2.08 }, //64
                    new VenueManager { UserName = "venuemanager23@test.com", Email = "venuemanager18@test.com", County = "York", Town = "Fossgate", Latitude = 53.96, Longitude = -1.08 }, //65
                    new VenueManager { UserName = "venuemanager24@test.com", Email = "venuemanager19@test.com", County = "Belfast", Town = "Titanic Quarter", Latitude = 54.61, Longitude = -5.91 }, //66
                    new VenueManager { UserName = "venuemanager25@test.com", Email = "venuemanager20@test.com", County = "Dublin", Town = "Grafton Street", Latitude = 53.34, Longitude = -6.26 }, //67
                    new VenueManager { UserName = "venuemanager26@test.com", Email = "venuemanager21@test.com", County = "Norwich", Town = "Magdalen Street", Latitude = 52.63, Longitude = 1.30 }, //68
                    new VenueManager { UserName = "venuemanager27@test.com", Email = "venuemanager22@test.com", County = "Exeter", Town = "Quay", Latitude = 50.72, Longitude = -3.53 }, //69
                    new VenueManager { UserName = "venuemanager28@test.com", Email = "venuemanager23@test.com", County = "Southampton", Town = "Bargate", Latitude = 50.9, Longitude = -1.4 }, //70
                    new VenueManager { UserName = "venuemanager29@test.com", Email = "venuemanager24@test.com", County = "Hull", Town = "Fruit Market", Latitude = 53.74, Longitude = -0.34 }, //71
                    new VenueManager { UserName = "venuemanager30@test.com", Email = "venuemanager25@test.com", County = "Plymouth", Town = "Barbican", Latitude = 50.37, Longitude = -4.14 }, //72
                    new VenueManager { UserName = "venuemanager31@test.com", Email = "venuemanager26@test.com", County = "Swansea", Town = "Mumbles", Latitude = 51.58, Longitude = -3.98 }, //73
                    new VenueManager { UserName = "venuemanager32@test.com", Email = "venuemanager27@test.com", County = "Inverness", Town = "Crown", Latitude = 57.48, Longitude = -4.23 }, //74
                    new VenueManager { UserName = "venuemanager33@test.com", Email = "venuemanager28@test.com", County = "Stirling", Town = "Causewayhead", Latitude = 56.15, Longitude = -3.93 }, //75
                    new VenueManager { UserName = "venuemanager34@test.com", Email = "venuemanager29@test.com", County = "Dundee", Town = "Seagate", Latitude = 56.47, Longitude = -2.87 }, //76
                    new VenueManager { UserName = "venuemanager35@test.com", Email = "venuemanager30@test.com", County = "Coventry", Town = "Far Gosford Street", Latitude = 52.41, Longitude = -1.5 } //77
                };


                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Password11!");
                    await userManager.AddToRoleAsync(user, user.GetType().Name.Replace("ApplicationUser", ""));
                }
            }

            // Genres
            if (!context.Genres.Any())
            {
                var genres = new Genre[]
                {
                    new Genre { Name = "Rock" }, new Genre { Name = "Pop" }, new Genre { Name = "Jazz" },
                    new Genre { Name = "Hip-Hop" }, new Genre { Name = "Electronic" }, new Genre { Name = "Indie" }
                };
                context.Genres.AddRange(genres);
                await context.SaveChangesAsync();
            }

            // Artists
            if (!context.Artists.Any())
            {
                var artists = new Artist[]
                {
                    new Artist { UserId = 8, Name = "The Rockers", About = "A thrilling rock band", ImageUrl = "rockers.jpg" },
                    new Artist { UserId = 9, Name = "Indie Vibes", About = "Smooth indie tunes", ImageUrl = "indievibes.jpg" },
                    new Artist { UserId = 10, Name = "Electronic Pulse", About = "Pumping electronic beats", ImageUrl = "electronicpulse.jpg" },
                    new Artist { UserId = 11, Name = "Hip-Hop Flow", About = "Smooth hip-hop beats", ImageUrl = "hiphopflow.jpg" },
                    new Artist { UserId = 12, Name = "Jazz Masters", About = "Timeless jazz performances", ImageUrl = "jazzmaster.jpg" },
                    new Artist { UserId = 13, Name = "Camden Punks", About = "High-energy punk from London", ImageUrl = "camdenpunks.jpg" },
                    new Artist { UserId = 14, Name = "Manchester Groove", About = "Indie soul vibes from Manchester", ImageUrl = "mancgroove.jpg" },
                    new Artist { UserId = 15, Name = "Birmingham Blues Express", About = "Authentic blues from the Midlands", ImageUrl = "brumblues.jpg" },
                    new Artist { UserId = 16, Name = "Leith Folk Revival", About = "Traditional folk band from Edinburgh", ImageUrl = "leithfolk.jpg" },
                    new Artist { UserId = 17, Name = "Merseybeat Revival", About = "Liverpool’s best Beatles tribute", ImageUrl = "merseybeat.jpg" },
                    new Artist { UserId = 18, Name = "Leeds Indie Collective", About = "Leeds' rising indie rock group", ImageUrl = "leedsindie.jpg" },
                    new Artist { UserId = 19, Name = "Glasgow Sound System", About = "Fusing electronic with Scottish folk", ImageUrl = "glasgowsound.jpg" },
                    new Artist { UserId = 20, Name = "Sheffield Steel Band", About = "Heavy riffs from Sheffield", ImageUrl = "sheffieldsteel.jpg" },
                    new Artist { UserId = 21, Name = "Nottingham Synthwave", About = "Retro-futuristic electronic sounds", ImageUrl = "nottssynth.jpg" },
                    new Artist { UserId = 22, Name = "Bristol Dub Club", About = "Reggae and dub from the South", ImageUrl = "bristoldub.jpg" },
                    new Artist { UserId = 23, Name = "Brighton Sea Shanties", About = "Traditional and modern sea shanties", ImageUrl = "brightonshanties.jpg" },
                    new Artist { UserId = 24, Name = "Cardiff Rockers", About = "Hard rock anthems from Wales", ImageUrl = "cardiffrock.jpg" },
                    new Artist { UserId = 25, Name = "Newcastle Rebels", About = "Alt-rock from the North East", ImageUrl = "newcastlerebels.jpg" },
                    new Artist { UserId = 26, Name = "Oxford Orchestral", About = "Classical music meets modern production", ImageUrl = "oxfordorchestra.jpg" },
                    new Artist { UserId = 27, Name = "Cambridge Jazz Quartet", About = "Smooth jazz with an intellectual touch", ImageUrl = "cambridgejazz.jpg" },
                    new Artist { UserId = 28, Name = "Bath Classical Ensemble", About = "Classical and baroque musicians", ImageUrl = "bathensemble.jpg" },
                    new Artist { UserId = 29, Name = "Aberdeen Pipes & Drums", About = "Scottish traditional pipe band", ImageUrl = "aberdeenpipes.jpg" },
                    new Artist { UserId = 30, Name = "York Alternative Sounds", About = "Experimental and alt-indie music", ImageUrl = "yorkalternative.jpg" },
                    new Artist { UserId = 31, Name = "Belfast Celtic Folk", About = "Irish and Celtic-inspired folk music", ImageUrl = "belfastfolk.jpg" },
                    new Artist { UserId = 32, Name = "Dublin Acoustic Sessions", About = "Raw and heartfelt acoustic tunes", ImageUrl = "dublinacoustic.jpg" },
                    new Artist { UserId = 33, Name = "Norwich Garage Rock", About = "High-energy garage punk from Norwich", ImageUrl = "norwichgarage.jpg" },
                    new Artist { UserId = 34, Name = "Exeter Prog Rock", About = "Mind-bending progressive rock", ImageUrl = "exeterprog.jpg" },
                    new Artist { UserId = 35, Name = "Southampton Pop Collective", About = "Mainstream pop with a modern touch", ImageUrl = "southamptonpop.jpg" },
                    new Artist { UserId = 36, Name = "Hull Hard Metal", About = "Heavy metal from Hull", ImageUrl = "hullmetal.jpg" },
                    new Artist { UserId = 37, Name = "Plymouth Surf Rockers", About = "Surf rock revival from the South West", ImageUrl = "plymouthsurf.jpg" },
                    new Artist { UserId = 38, Name = "Swansea Grunge Revival", About = "Bringing back 90s grunge", ImageUrl = "swanseagrunge.jpg" },
                    new Artist { UserId = 39, Name = "Inverness Celtic Band", About = "Scottish folk with a rock edge", ImageUrl = "invernessceltic.jpg" },
                    new Artist { UserId = 40, Name = "Stirling Blues Trio", About = "Deep blues with a soulful feel", ImageUrl = "stirlingblues.jpg" },
                    new Artist { UserId = 41, Name = "Dundee Electronic Beats", About = "Electro and dance music from Scotland", ImageUrl = "dundeeelectronic.jpg" },
                    new Artist { UserId = 42, Name = "Coventry Reggae Crew", About = "Reggae and dub from the Midlands", ImageUrl = "coventryreggae.jpg" }
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
                    new ArtistGenre { ArtistId = 2, GenreId = 2 },
                    new ArtistGenre { ArtistId = 3, GenreId = 5 },
                    new ArtistGenre { ArtistId = 4, GenreId = 4 },
                    new ArtistGenre { ArtistId = 5, GenreId = 3 }
                };
                context.ArtistGenres.AddRange(artistGenres);
                await context.SaveChangesAsync();
            }

            // Venues
            if (!context.Venues.Any())
            {
                var venues = new Venue[]
                {
                    new Venue { UserId = 43, Name = "The Grand Venue", About = "Premier event venue in Leatherhead", ImageUrl = "grandvenue.jpg", Approved = true },
                    new Venue {UserId = 44, Name = "Redhill Hall", About = "Historic hall for intimate gigs", ImageUrl = "redhillhall.jpg", Approved = true},
                    new Venue {UserId = 45, Name = "Weybridge Pavilion", About = "Modern space for concerts", ImageUrl = "weybridgepavilon.jpg", Approved = true},
                    new Venue {UserId = 46, Name = "Cobham Arts Centre", About = "Cultural hub for arts and music", ImageUrl = "cobhamarts.jpg", Approved = true},
                    new Venue {UserId = 47, Name = "Chertsey Arena", About = "Large arena for big events", ImageUrl = "chertseyarena.jpg", Approved = true},
                    new Venue { UserId = 48, Name = "Camden Electric Ballroom", About = "Iconic music venue in London", ImageUrl = "camdenballroom.jpg", Approved = true },
                    new Venue { UserId = 49, Name = "Manchester Night & Day Café", About = "A staple in the Northern Quarter", ImageUrl = "manchesternightday.jpg", Approved = true },
                    new Venue { UserId = 50, Name = "Birmingham O2 Institute", About = "Popular venue in Digbeth", ImageUrl = "birminghamo2.jpg", Approved = true },
                    new Venue { UserId = 51, Name = "Edinburgh Usher Hall", About = "Scotland’s famous concert hall", ImageUrl = "edinburghusher.jpg", Approved = true },
                    new Venue { UserId = 52, Name = "Liverpool Philharmonic Hall", About = "World-class performances in Liverpool", ImageUrl = "liverpoolphilharmonic.jpg", Approved = true },
                    new Venue { UserId = 53, Name = "Leeds Brudenell Social Club", About = "Indie hotspot in Leeds", ImageUrl = "leedsbrudenell.jpg", Approved = true },
                    new Venue { UserId = 54, Name = "Glasgow Barrowland Ballroom", About = "A historic music venue", ImageUrl = "glasgowbarrowland.jpg", Approved = true },
                    new Venue { UserId = 55, Name = "Sheffield Leadmill", About = "Sheffield’s oldest live venue", ImageUrl = "sheffieldleadmill.jpg", Approved = true },
                    new Venue { UserId = 56, Name = "Nottingham Rock City", About = "Nottingham’s home for rock gigs", ImageUrl = "nottinghamrockcity.jpg", Approved = true },
                    new Venue { UserId = 57, Name = "Bristol Thekla", About = "A floating venue on Bristol’s harborside", ImageUrl = "bristolthekla.jpg", Approved = true },
                    new Venue { UserId = 58, Name = "Brighton Concorde 2", About = "Legendary beachfront club", ImageUrl = "brightonconcorde2.jpg", Approved = true },
                    new Venue { UserId = 59, Name = "Cardiff Tramshed", About = "A vibrant venue in Cardiff", ImageUrl = "cardifftramshed.jpg", Approved = true },
                    new Venue { UserId = 60, Name = "Newcastle O2 Academy", About = "A key venue for live gigs", ImageUrl = "newcastleo2.jpg", Approved = true },
                    new Venue { UserId = 61, Name = "Oxford O2 Academy", About = "Popular music venue in Oxford", ImageUrl = "oxfordo2.jpg", Approved = true },
                    new Venue { UserId = 62, Name = "Cambridge Corn Exchange", About = "A historic venue in Cambridge", ImageUrl = "cambridgecornexchange.jpg", Approved = true },
                    new Venue { UserId = 63, Name = "Bath Komedia", About = "Live music & comedy in Bath", ImageUrl = "bathkomedia.jpg", Approved = true },
                    new Venue { UserId = 64, Name = "Aberdeen The Lemon Tree", About = "Aberdeen’s arts & music hub", ImageUrl = "aberdeenlemontree.jpg", Approved = true },
                    new Venue { UserId = 65, Name = "York Barbican", About = "A premier venue in York", ImageUrl = "yorkbarbican.jpg", Approved = true },
                    new Venue { UserId = 66, Name = "Belfast Limelight", About = "An iconic venue in Belfast", ImageUrl = "belfastlimelight.jpg", Approved = true },
                    new Venue { UserId = 67, Name = "Dublin Vicar Street", About = "Dublin’s top live performance space", ImageUrl = "dublinvicarstreet.jpg", Approved = true },
                    new Venue { UserId = 68, Name = "Norwich Waterfront", About = "Live gigs in Norwich", ImageUrl = "norwichwaterfront.jpg", Approved = true },
                    new Venue { UserId = 69, Name = "Exeter Phoenix", About = "Art, theatre & music in Exeter", ImageUrl = "exeterphoenix.jpg", Approved = true },
                    new Venue { UserId = 70, Name = "Southampton Engine Rooms", About = "An intimate venue for live music", ImageUrl = "southamptonengine.jpg", Approved = true },
                    new Venue { UserId = 71, Name = "Hull The Welly Club", About = "Hull’s favourite club & venue", ImageUrl = "hullwellyclub.jpg", Approved = true },
                    new Venue { UserId = 72, Name = "Plymouth Junction", About = "Bringing live music to Plymouth", ImageUrl = "plymouthjunction.jpg", Approved = true },
                    new Venue { UserId = 73, Name = "Swansea Sin City", About = "A must-visit for gigs in Swansea", ImageUrl = "swanseasincity.jpg", Approved = true },
                    new Venue { UserId = 74, Name = "Inverness Ironworks", About = "The Highlands’ premier venue", ImageUrl = "invernessironworks.jpg", Approved = true },
                    new Venue { UserId = 75, Name = "Stirling Albert Halls", About = "Hosting live acts since the 1800s", ImageUrl = "stirlingalberthalls.jpg", Approved = true },
                    new Venue { UserId = 76, Name = "Dundee Fat Sams", About = "Dundee’s biggest club & venue", ImageUrl = "dundeefatsams.jpg", Approved = true },
                    new Venue { UserId = 77, Name = "Coventry Empire", About = "Live music and club nights", ImageUrl = "coventryempire.jpg", Approved = true }
                };
                context.Venues.AddRange(venues);
                await context.SaveChangesAsync();
            }

            // Venue Images
            if (!context.VenueImages.Any())
            {
                var venueImages = new VenueImage[]
                {
                    new VenueImage { VenueId = 1, Url = "venue1_1.jpg" },
                    new VenueImage { VenueId = 1, Url = "venue1_2.jpg" },
                    new VenueImage { VenueId = 2, Url = "venue2_1.jpg" },
                    new VenueImage { VenueId = 3, Url = "venue3_1.jpg" }
                };
                context.VenueImages.AddRange(venueImages);
                await context.SaveChangesAsync();
            }

            // Listings
            if (!context.Listings.Any())
            {
                var listings = new Listing[]
                {
                    new Listing { VenueId = 1, StartDate = new DateTime(2024, 3, 15, 19, 0, 0), EndDate = new DateTime(2024, 3, 15, 22, 0, 0), Pay = 200 },
                    new Listing { VenueId = 2, StartDate = new DateTime(2025, 5, 10, 20, 0, 0), EndDate = new DateTime(2025, 5, 10, 23, 0, 0), Pay = 300 },
                    new Listing { VenueId = 3, StartDate = new DateTime(2024, 1, 10, 18, 0, 0), EndDate = new DateTime(2024, 1, 10, 20, 0, 0), Pay = 150 },
                    new Listing { VenueId = 4, StartDate = new DateTime(2025, 6, 20, 18, 0, 0), EndDate = new DateTime(2025, 6, 20, 21, 0, 0), Pay = 250 },
                    new Listing { VenueId = 5, StartDate = new DateTime(2024, 4, 5, 20, 0, 0), EndDate = new DateTime(2024, 4, 5, 23, 0, 0), Pay = 275 }
                };
                context.Listings.AddRange(listings);
                await context.SaveChangesAsync();
            }

            // ListingGenres
            if (!context.ListingGenres.Any())
            {
                var listingGenres = new ListingGenre[]
                {
                new ListingGenre { ListingId = 1, GenreId = 1 }, // Rock for Listing 1
                new ListingGenre { ListingId = 1, GenreId = 2 }, // Pop for Listing 1
                new ListingGenre { ListingId = 2, GenreId = 5 }, // Electronic for Listing 2
                new ListingGenre { ListingId = 3, GenreId = 3 }, // Jazz for Listing 3
                new ListingGenre { ListingId = 4, GenreId = 4 }, // Hip-Hop for Listing 4
                new ListingGenre { ListingId = 5, GenreId = 6 }  // Indie for Listing 5
                        };
                        context.ListingGenres.AddRange(listingGenres);
                        await context.SaveChangesAsync();
            }

            // Listing Applications
            if (!context.ListingApplications.Any())
            {
                var listingApplications = new ListingApplication[]
                {
                    new ListingApplication { ArtistId = 1, ListingId = 1 },
                    new ListingApplication { ArtistId = 2, ListingId = 2 },
                    new ListingApplication { ArtistId = 3, ListingId = 3 },
                    new ListingApplication { ArtistId = 4, ListingId = 4 },
                    new ListingApplication { ArtistId = 5, ListingId = 5 }
                };
                context.ListingApplications.AddRange(listingApplications);
                await context.SaveChangesAsync();
            }

            // Events
            if (!context.Events.Any())
            {
                var events = new Event[]
                {
                    new Event { ApplicationId = 1, Name = "Rock Night", Price = 15, TotalTickets = 100, AvailableTickets = 20, Posted = true },
                    new Event { ApplicationId = 2, Name = "Indie Evening", Price = 12, TotalTickets = 80, AvailableTickets = 80, Posted = false },
                    new Event { ApplicationId = 3, Name = "Jazz Gala", Price = 18, TotalTickets = 120, AvailableTickets = 0, Posted = true },
                    new Event { ApplicationId = 4, Name = "Electronic Bash", Price = 20, TotalTickets = 150, AvailableTickets = 50, Posted = true },
                    new Event { ApplicationId = 5, Name = "Hip-Hop Fest", Price = 10, TotalTickets = 200, AvailableTickets = 100, Posted = true }
                };
                context.Events.AddRange(events);
                await context.SaveChangesAsync();
            }

            // Tickets
            if (!context.Tickets.Any())
            {
                var tickets = new Ticket[]
                {
                    new Ticket { UserId = 2, EventId = 1, PurchaseDate = DateTime.Now },
                    new Ticket { UserId = 3, EventId = 1, PurchaseDate = DateTime.Now },
                    new Ticket { UserId = 2, EventId = 3, PurchaseDate = DateTime.Now }
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
                    new Review { TicketId = 2, Stars = 5, Details = "Loved every moment!" }
                };
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }

            // Messages
            if (!context.Messages.Any())
            {
                var messages = new Message[]
                {
                    new Message { FromUserId = 2, ToUserId = 9, Content = "Interested in your venue!", SentDate = DateTime.Now, Read = false },
                    new Message { FromUserId = 4, ToUserId = 10, Content = "Looking for a booking slot.", SentDate = DateTime.Now, Read = true }
                };
                context.Messages.AddRange(messages);
                await context.SaveChangesAsync();
            }

            // Purchases
            if (!context.Purchases.Any())
            {
                var purchases = new Purchase[]
                {
                    new Purchase { FromUserId = 2, ToUserId = 1, TransactionId = Guid.NewGuid().ToString(), Amount = 150, Type = "Ticket", Status = "Completed", CreatedAt = DateTime.Now },
                    new Purchase { FromUserId = 3, ToUserId = 1, TransactionId = Guid.NewGuid().ToString(), Amount = 275, Type = "Ticket", Status = "Completed", CreatedAt = DateTime.Now }
                };
                context.Purchases.AddRange(purchases);
                await context.SaveChangesAsync();
            }
        }
    }
}
