using Core.Entities;
using Core.Entities.Identity;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
                    new ArtistManager { UserName = "artistmanager6@test.com", Email = "artistmanager6@test.com", County = "London", Town = "Camden", Latitude = 51.53, Longitude = -0.13 }, //13
                    new ArtistManager { UserName = "artistmanager7@test.com", Email = "artistmanager7@test.com", County = "Manchester", Town = "Salford", Latitude = 53.48, Longitude = -2.25 }, //14
                    new ArtistManager { UserName = "artistmanager8@test.com", Email = "artistmanager8@test.com", County = "Birmingham", Town = "Digbeth", Latitude = 52.47, Longitude = -1.88 }, //15
                    new ArtistManager { UserName = "artistmanager9@test.com", Email = "artistmanager9@test.com", County = "Edinburgh", Town = "Leith", Latitude = 55.98, Longitude = -3.17 }, //16
                    new ArtistManager { UserName = "artistmanager10@test.com", Email = "artistmanager10@test.com", County = "Liverpool", Town = "Baltic Triangle", Latitude = 53.39, Longitude = -2.98 }, //17
                    new ArtistManager { UserName = "artistmanager11@test.com", Email = "artistmanager11@test.com", County = "Leeds", Town = "Headingley", Latitude = 53.82, Longitude = -1.58 }, //18
                    new ArtistManager { UserName = "artistmanager12@test.com", Email = "artistmanager12@test.com", County = "Glasgow", Town = "West End", Latitude = 55.87, Longitude = -4.29 }, //19
                    new ArtistManager { UserName = "artistmanager13@test.com", Email = "artistmanager13@test.com", County = "Sheffield", Town = "Kelham Island", Latitude = 53.39, Longitude = -1.46 }, //20
                    new ArtistManager { UserName = "artistmanager14@test.com", Email = "artistmanager14@test.com", County = "Nottingham", Town = "Lace Market", Latitude = 52.95, Longitude = -1.14 }, //21
                    new ArtistManager { UserName = "artistmanager15@test.com", Email = "artistmanager150@test.com", County = "Bristol", Town = "Stokes Croft", Latitude = 51.46, Longitude = -2.59 }, //22
                    new ArtistManager { UserName = "artistmanager16@test.com", Email = "artistmanager16@test.com", County = "Brighton", Town = "Kemptown", Latitude = 50.82, Longitude = -0.13 }, //23
                    new ArtistManager { UserName = "artistmanager17@test.com", Email = "artistmanager17@test.com", County = "Cardiff", Town = "Cathays", Latitude = 51.49, Longitude = -3.17 }, //24
                    new ArtistManager { UserName = "artistmanager18@test.com", Email = "artistmanager18@test.com", County = "Newcastle", Town = "Jesmond", Latitude = 54.99, Longitude = -1.61 }, //25
                    new ArtistManager { UserName = "artistmanager19@test.com", Email = "artistmanager19@test.com", County = "Oxford", Town = "Jericho", Latitude = 51.76, Longitude = -1.26 }, //26
                    new ArtistManager { UserName = "artistmanager20@test.com", Email = "artistmanager20@test.com", County = "Cambridge", Town = "Mill Road", Latitude = 52.19, Longitude = 0.13 }, //27
                    new ArtistManager { UserName = "artistmanager21@test.com", Email = "artistmanager21@test.com", County = "Bath", Town = "Widcombe", Latitude = 51.38, Longitude = -2.36 }, //28
                    new ArtistManager { UserName = "artistmanager22@test.com", Email = "artistmanager22@test.com", County = "Aberdeen", Town = "Old Aberdeen", Latitude = 57.17, Longitude = -2.1 },  //29
                    new ArtistManager { UserName = "artistmanager23@test.com", Email = "artistmanager23@test.com", County = "York", Town = "The Shambles", Latitude = 53.96, Longitude = -1.08 }, //30
                    new ArtistManager { UserName = "artistmanager24@test.com", Email = "artistmanager24@test.com", County = "Belfast", Town = "Cathedral Quarter", Latitude = 54.6, Longitude = -5.93 }, //31
                    new ArtistManager { UserName = "artistmanager25@test.com", Email = "artistmanager25@test.com", County = "Dublin", Town = "Temple Bar", Latitude = 53.34, Longitude = -6.27 }, //32
                    new ArtistManager { UserName = "artistmanager26@test.com", Email = "artistmanager26@test.com", County = "Norwich", Town = "Tombland", Latitude = 52.63, Longitude = 1.3 }, //33
                    new ArtistManager { UserName = "artistmanager27@test.com", Email = "artistmanager27@test.com", County = "Exeter", Town = "St Sidwell's", Latitude = 50.73, Longitude = -3.53 }, //34
                    new ArtistManager { UserName = "artistmanager28@test.com", Email = "artistmanager28@test.com", County = "Southampton", Town = "Ocean Village", Latitude = 50.9, Longitude = -1.4 }, //35
                    new ArtistManager { UserName = "artistmanager29@test.com", Email = "artistmanager29@test.com", County = "Hull", Town = "Old Town", Latitude = 53.74, Longitude = -0.33 }, //36
                    new ArtistManager { UserName = "artistmanager30@test.com", Email = "artistmanager30@test.com", County = "Plymouth", Town = "The Hoe", Latitude = 50.37, Longitude = -4.14 }, //37
                    new ArtistManager { UserName = "artistmanager31@test.com", Email = "artistmanager31@test.com", County = "Swansea", Town = "Uplands", Latitude = 51.62, Longitude = -3.94 }, //38
                    new ArtistManager { UserName = "artistmanager32@test.com", Email = "artistmanager32@test.com", County = "Inverness", Town = "Dalneigh", Latitude = 57.48, Longitude = -4.23 }, //39
                    new ArtistManager { UserName = "artistmanager33@test.com", Email = "artistmanager33@test.com", County = "Stirling", Town = "Bridge of Allan", Latitude = 56.15, Longitude = -3.93 }, //40
                    new ArtistManager { UserName = "artistmanager34@test.com", Email = "artistmanager34@test.com", County = "Dundee", Town = "Broughty Ferry", Latitude = 56.47, Longitude = -2.87 }, //41
                    new ArtistManager { UserName = "artistmanager35@test.com", Email = "artistmanager35@test.com", County = "Coventry", Town = "Earlsdon", Latitude = 52.40, Longitude = -1.52 }, //42
                    // Venue Managers
                    new VenueManager { UserName = "venuemanager1@test.com", Email = "venuemanager1@test.com", County = "Surrey", Town = "Leatherhead", Latitude = 51.3, Longitude = -0.3 }, //43
                    new VenueManager { UserName = "venuemanager2@test.com", Email = "venuemanager2@test.com", County = "Surrey", Town = "Redhill", Latitude = 51.23, Longitude = -0.17 }, //44
                    new VenueManager { UserName = "venuemanager3@test.com", Email = "venuemanager3@test.com", County = "Surrey", Town = "Weybridge", Latitude = 51.38, Longitude = -0.46 }, //45
                    new VenueManager { UserName = "venuemanager4@test.com", Email = "venuemanager4@test.com", County = "Surrey", Town = "Cobham", Latitude = 51.32, Longitude = -0.46 }, //46
                    new VenueManager { UserName = "venuemanager5@test.com", Email = "venuemanager5@test.com", County = "Surrey", Town = "Chertsey", Latitude = 51.39, Longitude = -0.5 }, //47
                    new VenueManager { UserName = "venuemanager6@test.com", Email = "venuemanager6@test.com", County = "London", Town = "Camden", Latitude = 51.53, Longitude = -0.13 }, //48
                    new VenueManager { UserName = "venuemanager7@test.com", Email = "venuemanager7@test.com", County = "Manchester", Town = "Northern Quarter", Latitude = 53.48, Longitude = -2.23 }, //49
                    new VenueManager { UserName = "venuemanager8@test.com", Email = "venuemanager8@test.com", County = "Birmingham", Town = "Jewellery Quarter", Latitude = 52.48, Longitude = -1.91 }, //50
                    new VenueManager { UserName = "venuemanager9@test.com", Email = "venuemanager9@test.com", County = "Edinburgh", Town = "Old Town", Latitude = 55.95, Longitude = -3.19 }, //51
                    new VenueManager { UserName = "venuemanager10@test.com", Email = "venuemanager10@test.com", County = "Liverpool", Town = "Cavern Quarter", Latitude = 53.41, Longitude = -2.99 }, //52
                    new VenueManager { UserName = "venuemanager11@test.com", Email = "venuemanager11@test.com", County = "Leeds", Town = "Call Lane", Latitude = 53.79, Longitude = -1.54 }, //53
                    new VenueManager { UserName = "venuemanager12@test.com", Email = "venuemanager12@test.com", County = "Glasgow", Town = "Merchant City", Latitude = 55.86, Longitude = -4.24 }, //54
                    new VenueManager { UserName = "venuemanager13@test.com", Email = "venuemanager13@test.com", County = "Sheffield", Town = "Ecclesall Road", Latitude = 53.38, Longitude = -1.50 }, //55
                    new VenueManager { UserName = "venuemanager14@test.com", Email = "venuemanager14@test.com", County = "Nottingham", Town = "Hockley", Latitude = 52.95, Longitude = -1.14 }, //56
                    new VenueManager { UserName = "venuemanager15@test.com", Email = "venuemanager15@test.com", County = "Bristol", Town = "Harbourside", Latitude = 51.45, Longitude = -2.60 }, //57
                    new VenueManager { UserName = "venuemanager16@test.com", Email = "venuemanager16@test.com", County = "Brighton", Town = "The Lanes", Latitude = 50.82, Longitude = -0.14 }, //58
                    new VenueManager { UserName = "venuemanager17@test.com", Email = "venuemanager17@test.com", County = "Cardiff", Town = "Riverside", Latitude = 51.48, Longitude = -3.18 }, //59
                    new VenueManager { UserName = "venuemanager18@test.com", Email = "venuemanager18@test.com", County = "Newcastle", Town = "Quayside", Latitude = 54.97, Longitude = -1.60 }, //60
                    new VenueManager { UserName = "venuemanager19@test.com", Email = "venuemanager19@test.com", County = "Oxford", Town = "Cowley", Latitude = 51.73, Longitude = -1.22 }, //61
                    new VenueManager { UserName = "venuemanager20@test.com", Email = "venuemanager20@test.com", County = "Cambridge", Town = "Chesterton", Latitude = 52.22, Longitude = 0.14 }, //62
                    new VenueManager { UserName = "venuemanager21@test.com", Email = "venuemanager21@test.com", County = "Bath", Town = "Bear Flat", Latitude = 51.37, Longitude = -2.36 }, //63
                    new VenueManager { UserName = "venuemanager22@test.com", Email = "venuemanager22@test.com", County = "Aberdeen", Town = "Footdee", Latitude = 57.15, Longitude = -2.08 }, //64
                    new VenueManager { UserName = "venuemanager23@test.com", Email = "venuemanager23@test.com", County = "York", Town = "Fossgate", Latitude = 53.96, Longitude = -1.08 }, //65
                    new VenueManager { UserName = "venuemanager24@test.com", Email = "venuemanager24@test.com", County = "Belfast", Town = "Titanic Quarter", Latitude = 54.61, Longitude = -5.91 }, //66
                    new VenueManager { UserName = "venuemanager25@test.com", Email = "venuemanager25@test.com", County = "Dublin", Town = "Grafton Street", Latitude = 53.34, Longitude = -6.26 }, //67
                    new VenueManager { UserName = "venuemanager26@test.com", Email = "venuemanager26@test.com", County = "Norwich", Town = "Magdalen Street", Latitude = 52.63, Longitude = 1.30 }, //68
                    new VenueManager { UserName = "venuemanager27@test.com", Email = "venuemanager27@test.com", County = "Exeter", Town = "Quay", Latitude = 50.72, Longitude = -3.53 }, //69
                    new VenueManager { UserName = "venuemanager28@test.com", Email = "venuemanager28@test.com", County = "Southampton", Town = "Bargate", Latitude = 50.9, Longitude = -1.4 }, //70
                    new VenueManager { UserName = "venuemanager29@test.com", Email = "venuemanager29@test.com", County = "Hull", Town = "Fruit Market", Latitude = 53.74, Longitude = -0.34 }, //71
                    new VenueManager { UserName = "venuemanager30@test.com", Email = "venuemanager30@test.com", County = "Plymouth", Town = "Barbican", Latitude = 50.37, Longitude = -4.14 }, //72
                    new VenueManager { UserName = "venuemanager31@test.com", Email = "venuemanager31@test.com", County = "Swansea", Town = "Mumbles", Latitude = 51.58, Longitude = -3.98 }, //73
                    new VenueManager { UserName = "venuemanager32@test.com", Email = "venuemanager32@test.com", County = "Inverness", Town = "Crown", Latitude = 57.48, Longitude = -4.23 }, //74
                    new VenueManager { UserName = "venuemanager33@test.com", Email = "venuemanager33@test.com", County = "Stirling", Town = "Causewayhead", Latitude = 56.15, Longitude = -3.93 }, //75
                    new VenueManager { UserName = "venuemanager34@test.com", Email = "venuemanager34@test.com", County = "Dundee", Town = "Seagate", Latitude = 56.47, Longitude = -2.87 }, //76
                    new VenueManager { UserName = "venuemanager35@test.com", Email = "venuemanager35@test.com", County = "Coventry", Town = "Far Gosford Street", Latitude = 52.41, Longitude = -1.5 } //77
                };


                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Password11!");
                    await userManager.AddToRoleAsync(user, user.GetType().Name.Replace("ApplicationUser", ""));
                }
            }

            //Preferences
            if(!context.Preferences.Any())
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

            // Artists
            if (!context.Artists.Any())
            {
                var artists = new Artist[]
                {
                    new Artist {
                        UserId = 8,
                        Name = "The Rockers",
                        About = "A high-energy rock band delivering explosive guitar riffs, thunderous drums, and electrifying live performances. With a sound rooted in classic rock and modern alternative influences, they bring anthems that shake stadiums and ignite crowds.",
                        ImageUrl = "rockers.jpg" },
                    new Artist {
                        UserId = 9,
                        Name = "Indie Vibes",
                        About = "A soulful and melodic indie band known for their breezy guitar tones, heartfelt lyrics, and chill yet uplifting soundscapes. Whether it’s a laid-back acoustic ballad or an upbeat indie anthem, their music is a perfect soundtrack for lazy afternoons and late-night nostalgia.",
                        ImageUrl = "indievibes.jpg" },
                    new Artist {
                        UserId = 10,
                        Name = "Electronic Pulse",
                        About = "A high-energy electronic music act, crafting pulsating beats, euphoric synth drops, and infectious dancefloor anthems. Their sound fuses house, trance, and EDM, making every track a sonic rush built for club nights and festival stages.",
                        ImageUrl = "electronicpulse.jpg" },
                    new Artist {
                        UserId = 11,
                        Name = "Hip-Hop Flow",
                        About = "A rhythm-driven hip-hop collective, blending smooth lyrical storytelling, deep basslines, and head-nodding beats. Their music is a mix of classic boom-bap, modern trap, and jazz-infused hip-hop, perfect for lovers of authentic and dynamic flows.",
                        ImageUrl = "hiphopflow.jpg" },
                    new Artist {
                        UserId = 12,
                        Name = "Jazz Masters",
                        About = "A world-class jazz ensemble, delivering timeless performances filled with intricate improvisations, swinging rhythms, and soulful melodies. Inspired by the legends of bebop, swing, and fusion, they keep the spirit of jazz alive with every note.",
                        ImageUrl = "jazzmaster.jpg" },
                    new Artist {
                        UserId = 13,
                        Name = "Always Punks",
                        About = "A high-octane punk band straight from London, serving up fast-paced riffs, rebellious anthems, and raw, unapologetic energy. With influences from classic UK punk and modern hardcore, they embody the spirit of DIY rebellion and underground chaos.",
                        ImageUrl = "alwayspunks.jpg" },
                    new Artist {
                        UserId = 14,
                        Name = "The Hollow Frequencies",
                        About = "A mysterious and atmospheric rock/indie band blending shoegaze textures, post-rock soundscapes, and eerie electronic elements. Their music creates a dreamlike experience, full of ethereal vocals, reverb-heavy guitars, and hypnotic beats. Fans of Radiohead, Slowdive, and Tame Impala will appreciate their unique sonic depth.",
                        ImageUrl = "hollowfrequencies.jpg" },
                    new Artist {
                        UserId = 15,
                        Name = "Neon Foxes",
                        About = "An electrifying indie rock outfit with a retro-futuristic twist. Known for their bright neon aesthetics, punchy guitar riffs, and synth-laden hooks, they seamlessly fuse new wave, alternative rock, and synth-pop. Imagine The Killers meets CHVRCHES, with a dash of 80s nostalgia.",
                        ImageUrl = "neonfoxes.jpg" },
                    new Artist {
                        UserId = 16,
                        Name = "Velvet Static",
                        About = "An alternative rock band with a grungy, raw energy and deep emotional lyricism. They mix 90s grunge, modern alt-rock, and electronic noise elements, creating a heavy yet melancholic sound. Think Nine Inch Nails meets Wolf Alice, wrapped in a haze of distortion and emotion.",
                        ImageUrl = "velvetstatic.jpg" },
                    new Artist {
                        UserId = 17,
                        Name = "Echo Bloom",
                        About = "A delicate fusion of indie folk, dream pop, and post-rock, Echo Bloom crafts beautifully cinematic, reverb-soaked melodies. Their sound is gentle yet expansive, full of lush harmonies and introspective lyrics. Perfect for fans of Fleet Foxes, Daughter, and Sigur Rós.",
                        ImageUrl = "echobloom.jpg" },
                    new Artist {
                        UserId = 18,
                        Name = "The Wild Chords",
                        About = "A high-energy rock band with classic rock roots and a punk-inspired edge. Their music is full of blazing guitar solos, anthemic choruses, and rebellious energy, echoing the sounds of Foo Fighters, The White Stripes, and The Black Keys. Pure stadium rock energy meets garage rock grit.",
                        ImageUrl = "wildchords.jpg" },
                    new Artist {
                        UserId = 19,
                        Name = "Glitch & Glow",
                        About = "A cutting-edge electropop duo that merges glitchy beats, shimmering synths, and futuristic melodies. Their music is playful yet deeply layered, drawing from hyperpop, synthwave, and experimental electronica. Think Grimes meets 100 gecs with a neon cyberpunk glow.",
                        ImageUrl = "glitchandglow.jpg" },
                    new Artist {
                        UserId = 20,
                        Name = "Sonic Mirage",
                        About = "A boundary-pushing artist blending ambient pop, chillwave, and experimental electronica. With dreamy vocal manipulations, warped synth textures, and hypnotic beats, their music transports listeners into an otherworldly sonic realm. Fans of James Blake, FKA twigs, and Bon Iver’s electronic work will feel at home.",
                        ImageUrl = "sonicmirage.jpg" },
                    new Artist {
                        UserId = 21,
                        Name = "Neon Echoes",
                        About = "An infectious pop-electronic project with a penchant for shiny hooks, bouncy basslines, and nostalgic 80s synth tones. They craft high-energy anthems perfect for late-night city drives and festival dancefloors. Think Dua Lipa meets The Weeknd’s After Hours era.",
                        ImageUrl = "neonechoes.jpg" },
                    new Artist {
                        UserId = 22,
                        Name = "Dreamwave Collective",
                        About = "A synthwave-inspired collective that blends retro-futuristic aesthetics with modern dance music. Their music is full of lush pads, pulsating basslines, and dreamy vocals, creating a nostalgic yet fresh soundscape. Ideal for fans of Tycho, M83, and Kavinsky.",
                        ImageUrl = "dreamwavecollective.jpg" },
                    new Artist {
                        UserId = 23,
                        Name = "Synth Pulse",
                        About = "A high-energy electro-house act that thrives on pounding beats, pulsing synth rhythms, and euphoric drops. Their music is engineered for massive club nights and festival main stages, blending influences from Daft Punk, Justice, and Deadmau5.",
                        ImageUrl = "synthpulse.jpg" },
                    new Artist {
                        UserId = 24,
                        Name = "The Brass Poets",
                        About = "A modern jazz-hip-hop fusion group, blending slick brass arrangements, spoken-word poetry, and jazzy boom-bap beats. Their music is both sophisticated and raw, reminiscent of Robert Glasper meets A Tribe Called Quest.",
                        ImageUrl = "brasspoets.jpg" },
                    new Artist {
                        UserId = 25,
                        Name = "Groove Alchemy",
                        About = "A genre-blending hip-hop, funk, and jazz ensemble known for their infectious grooves, soulful samples, and dynamic live instrumentation. They mix classic jazz with hip-hop storytelling, creating something that feels both vintage and contemporary. Fans of Anderson .Paak, J Dilla, and The Roots will love their sound.",
                        ImageUrl = "groovealchemy.jpg" },
                    new Artist {
                        UserId = 26,
                        Name = "Velvet Rhymes",
                        About = "A smooth and soulful hip-hop act incorporating laid-back jazz vibes, silky R&B vocals, and introspective lyricism. Their sound is moody and intimate, perfect for late-night contemplation. Think Common meets D’Angelo with a touch of lo-fi jazz-hop.",
                        ImageUrl = "velvetrhymes.jpg" },
                    new Artist {
                        UserId = 27,
                        Name = "The Lo-Fi Syndicate",
                        About = "A collective of producers, beatmakers, and instrumentalists crafting chilled-out, atmospheric lo-fi beats. Their sound is perfect for study sessions, rainy days, and meditative relaxation. They pull influences from J Dilla, Nujabes, and Flying Lotus.",
                        ImageUrl = "lofisyndicate.jpg" },
                    new Artist {
                        UserId = 28,
                        Name = "Beats & Blue Notes",
                        About = "A vibrant jazz-hip-hop crossover act, weaving swing-inspired horn sections, turntablism, and laid-back rap flows. They blend classic bebop energy with modern hip-hop rhythms, appealing to fans of Guru’s Jazzmatazz and Madlib’s Blue Note remixes.",
                        ImageUrl = "beatsbluenotes.jpg" },
                    new Artist {
                        UserId = 29,
                        Name = "Bass Pilots",
                        About = "A high-octane drum & bass DJ/producer duo delivering fast-paced, high-energy bass drops, intricate breakbeats, and futuristic soundscapes. Their sets keep the crowd moving non-stop. Think Chase & Status meets Noisia.",
                        ImageUrl = "basspilots.jpg" },
                    new Artist {
                        UserId = 30,
                        Name = "The Digital Prophets",
                        About = "A collective at the cutting edge of AI-infused electronic music, merging techno, house, and glitchy IDM elements. Their music feels like a prophecy of the future of club sound, drawing from Aphex Twin, Four Tet, and Richie Hawtin.",
                        ImageUrl = "digitalprophets.jpg" },
                    new Artist {
                        UserId = 31,
                        Name = "Neon Bass Theory",
                        About = "A futuristic bass music act, fusing deep dubstep, DnB, and cyberpunk aesthetics. Their music feels like stepping into a neon-lit sci-fi rave, with thick sub-bass and glitchy, mechanical beats. Think Rezz meets The Prodigy.",
                        ImageUrl = "neonbasstheory.jpg" },
                    new Artist {
                        UserId = 32,
                        Name = "Wavelength 303",
                        About = "A house/techno producer inspired by classic acid house and Detroit techno. With hypnotic 303 basslines, pulsating four-on-the-floor rhythms, and atmospheric textures, their sound pays homage to pioneers like Carl Cox, Daft Punk, and The Chemical Brothers.",
                        ImageUrl = "wavelength303.jpg" },
                    new Artist {
                        UserId = 33,
                        Name = "Gravity Loops",
                        About = "A deep house and future garage project that thrives on soulful vocal chops, atmospheric synth layers, and rhythmic house grooves. Their sound is perfect for sunset beach parties and underground club nights. Think Disclosure meets Burial.",
                        ImageUrl = "gravityloops.jpg" },
                    new Artist {
                        UserId = 34,
                        Name = "The Golden Reverie",
                        About = "A genre-fluid rock/pop collective that blends grand orchestral arrangements with stadium-sized indie anthems. Their sound is lush, cinematic, and emotionally powerful, akin to Coldplay, Arcade Fire, and Florence + The Machine.",
                        ImageUrl = "goldenreverie.jpg" },
                    new Artist {
                        UserId = 35,
                        Name = "Fable Sound",
                        About = "A mythical and storytelling-driven alt-rock band known for their fantastical lyricism, epic song structures, and progressive influences. Their music is an adventure, drawing comparisons to Muse, Coheed and Cambria, and Of Monsters and Men.",
                        ImageUrl = "fablesound.jpg" },
                    new Artist {
                        UserId = 36,
                        Name = "Moonlight Static",
                        About = "A lo-fi indie-pop band with a dreamy, melancholic touch. Their songs are nostalgic and introspective, with soft vocals, ambient synths, and laid-back guitar tones. Think Beach House, Cigarettes After Sex, and The xx.",
                        ImageUrl = "moonlightstatic.jpg" },
                    new Artist {
                        UserId = 37,
                        Name = "The Chromatics",
                        About = "A genre-blending synth-rock band that fuses post-punk, indie-pop, and 80s electronic influences. Their sound is both retro and modern, perfect for fans of The Cure, New Order, and Chromatics (the actual band!).",
                        ImageUrl = "thechromatics.jpg" },
                    new Artist {
                        UserId = 38,
                        Name = "Echo Reverberation",
                        About = "A band that thrives on psychedelic indie vibes, experimental production, and reverb-drenched guitars. Their music is hypnotic, hallucinatory, and atmospheric—think Tame Impala, My Bloody Valentine, and MGMT.",
                        ImageUrl = "echoreverberation.jpg" },
                    new Artist {
                        UserId = 39,
                        Name = "Midnight Reverie",
                        About = "A dreamy yet electrifying alt-rock band that blends atmospheric synth textures with soaring guitar riffs and emotionally charged vocals. Their sound moves effortlessly between melancholic ballads and euphoric anthems, creating a cinematic experience. Inspired by The War on Drugs, Wolf Alice, and The Killers, they thrive on nostalgia-laced melodies and expansive, reverb-drenched soundscapes. Their music is perfect for late-night drives and introspective moments.",
                        ImageUrl = "midnightreverie.jpg" },
                    new Artist {
                        UserId = 40,
                        Name = "Static Wolves",
                        About = "A gritty, high-energy rock band that combines garage rock rawness with alternative rock’s polished intensity. Their songs feature raspy, anthemic vocals, punchy drum patterns, and distorted, riff-heavy guitars that cut through the noise like a wild animal in the night. Fans of Royal Blood, Arctic Monkeys, and The White Stripes will love their swagger-filled, rebellious energy that feels both dangerous and electrifying.",
                        ImageUrl = "staticwolves.jpg" },
                    new Artist {
                        UserId = 41,
                        Name = "Echo Collapse",
                        About = "A post-punk revival band with a cinematic and introspective edge, Echo Collapse thrives on moody basslines, haunting vocals, and hypnotic drum patterns. Their sound is both melancholic and powerful, pulling inspiration from Interpol, The Cure, and Joy Division. With a dark yet melodic approach, they create music that resonates with outsiders, night wanderers, and those lost in thought.",
                        ImageUrl = "echocollapse.jpg" },
                    new Artist {
                        UserId = 42,
                        Name = "Violet Sundown",
                        About = "A psychedelic indie band that fuses alternative rock, dream pop, and shoegaze influences, creating a kaleidoscope of lush soundscapes. Their music is characterized by swirling guitars, hazy vocals, and hypnotic rhythms, pulling listeners into a trance-like state. Think Tame Impala meets Beach House, with a touch of My Bloody Valentine. Their sound is ethereal yet grounded, nostalgic yet futuristic, making them a favorite for deep thinkers and cosmic dreamers.",
                        ImageUrl = "violetsundown.jpg" }
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
                    new Venue { 
                        UserId = 43, 
                        Name = "The Grand Venue",
                        About = "Tucked away in the heart of Leatherhead, The Grand Venue has long been a cornerstone of the town’s cultural scene. Originally built as a Victorian-era community hall, it later transformed into an intimate venue for folk nights, jazz performances, and local theatre productions. With its wooden beam ceiling, vintage chandeliers, and a snug bar serving craft ales, it exudes an old-world charm. Today, it remains a go-to spot for grassroots artists and hosts everything from acoustic showcases to spoken word nights.", 
                        ImageUrl = "grandvenue.jpg", 
                        Approved = true },
                    new Venue {
                        UserId = 44, 
                        Name = "Redhill Hall", 
                        About = "Redhill Hall is a historic building that has served as a gathering place for musicians, poets, and artists since the late 1800s. Originally a town assembly hall, it was repurposed into a performance space in the 1970s, providing an intimate stage for indie bands, classical quartets, and local theatre productions. With its red-bricked exterior, arched windows, and candle-lit interior, the hall is both nostalgic and atmospheric—a hidden gem for folk nights and unplugged performances.", 
                        ImageUrl = "redhillhall.jpg", 
                        Approved = true},
                    new Venue {
                        UserId = 45, 
                        Name = "Weybridge Pavilion", 
                        About = "Originally a community center built in the 1950s, Weybridge Pavilion has become a beloved venue for up-and-coming indie bands and alternative rock groups. Its modest stage and open floor layout allow for intimate yet energetic performances, often bringing in local talent and traveling artists. On weekends, it doubles as a DIY art space, showcasing photography exhibitions and spoken-word events. Locals love it for its laid-back atmosphere, low lighting, and vintage posters lining the walls.", 
                        ImageUrl = "weybridgepavilon.jpg", 
                        Approved = true},
                    new Venue {
                        UserId = 46, 
                        Name = "Cobham Arts Centre",
                        About = "The Cobham Arts Centre was founded in the early 1990s by a group of artists who wanted to create a dedicated space for music, theatre, and visual arts. Built in a converted warehouse, it retains its industrial charm, with exposed brick walls, large arched windows, and a multipurpose stage that accommodates everything from classical recitals to electronic music nights. It’s a favorite among experimental musicians and alternative theatre groups, attracting a crowd that appreciates art in all forms.",
                        ImageUrl = "cobhamarts.jpg",
                        Approved = true},
                    new Venue {
                        UserId = 47, 
                        Name = "Chertsey Arena", 
                        About = "Unlike most small venues, Chertsey Arena was purpose-built in the 1980s as a regional music and performance venue. While it can hold larger crowds, it maintains a tight-knit community feel, regularly hosting tribute acts, battle of the bands, and grassroots punk shows. The venue is known for its dim neon lights, dark wood bar, and posters from past decades covering every inch of the walls.", 
                        ImageUrl = "chertseyarena.jpg", 
                        Approved = true},
                    new Venue { 
                        UserId = 48, 
                        Name = "Camden Electric Ballroom", 
                        About = "A legendary venue in London’s Camden Town, the Electric Ballroom has been a fixture of the alternative music scene since the 1950s. Originally a dance hall, it later became an iconic spot for punk, rock, and electronic gigs, with bands like The Clash and The Smiths once gracing its stage. Today, the venue retains its underground charm, with a graffiti-covered exterior, a packed standing area, and a history steeped in counterculture.", 
                        ImageUrl = "camdenballroom.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 49, 
                        Name = "Manchester Night & Day Café", 
                        About = "A staple of Manchester’s Northern Quarter, Night & Day Café has been at the heart of the city’s indie music scene since the 1990s. Known for launching the careers of Britpop and post-punk revival bands, this cozy, café-style venue offers a laid-back atmosphere by day and electric energy by night. With wood-paneled walls, vintage gig posters, and low-hanging bulbs illuminating the stage, it’s the perfect spot for emerging bands and intimate acoustic nights.", 
                        ImageUrl = "manchesternightday.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 50, 
                        Name = "Birmingham O2 Institute", 
                        About = "Housed in a former church, Birmingham’s O2 Institute is a stunning mix of gothic architecture and modern music culture. Built in the early 1900s, its arched ceilings and stained-glass windows provide a striking contrast to the rock, indie, and electronic acts that now take the stage. It’s a favorite among touring artists who love its intimate yet grand feel.", 
                        ImageUrl = "birminghamo2.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 51, 
                        Name = "Edinburgh Usher Hall",
                        About = "One of Scotland’s most renowned venues, Usher Hall has been a premier location for classical concerts, jazz performances, and contemporary acts since the early 20th century. The venue’s ornate architecture, velvet drapes, and pristine acoustics make it a sought-after stage for musicians of all genres. While it leans towards orchestral and folk performances, it has also welcomed alternative rock and indie musicians over the years.", 
                        ImageUrl = "edinburghusher.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 52, 
                        Name = "Liverpool Philharmonic Hall", 
                        About = "A jewel of Liverpool’s music scene, the Philharmonic Hall was built in 1939 and is home to the Royal Liverpool Philharmonic Orchestra. Known for its exceptional acoustics, it attracts jazz musicians, orchestras, and even intimate rock performances. The venue’s classic Art Deco design, luxurious seating, and historic charm make it one of the most treasured cultural spaces in the city.",
                        ImageUrl = "liverpoolphilharmonic.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 53, 
                        Name = "Leeds Brudenell Social Club", 
                        About = "A legendary indie music venue that has remained authentically grassroots since its founding in 1913 as a working men’s club. Over the years, it evolved into a haven for DIY musicians, alternative rock bands, and underground artists. The club retains its community feel, with a no-frills bar, simple wooden seating, and a tiny, sweat-soaked stage where future stars are born. It’s the kind of place where intimacy and raw energy define the experience.", 
                        ImageUrl = "leedsbrudenell.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 54, 
                        Name = "Glasgow Barrowland Ballroom",
                        About = "A historic music hall dating back to 1934, the Barrowland Ballroom is a Glaswegian institution. Once a dance hall for swing and jazz lovers, it now hosts some of the biggest indie and rock acts, yet still feels deeply connected to its blue-collar roots. With its retro neon sign, vintage Art Deco interior, and a bouncing wooden floor that vibrates with the crowd, it’s one of the most beloved live music venues in Scotland.",
                        ImageUrl = "glasgowbarrowland.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 55, 
                        Name = "Sheffield Leadmill", 
                        About = "Opening its doors in 1980, The Leadmill is Sheffield’s oldest live music venue and a launchpad for alternative bands, punk groups, and indie rockers. It has played host to early performances from Pulp, Arctic Monkeys, and The Killers, and its low ceilings, intimate stage, and sticky floors make it a true dive bar venue where every gig feels like a secret show. It’s loud, gritty, and full of character.", 
                        ImageUrl = "sheffieldleadmill.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 56, 
                        Name = "Nottingham Rock City",
                        About = "Since its opening in 1980, Rock City has earned a reputation as one of the UK’s most iconic rock venues. Hosting everything from metal to alternative rock, its graffiti-covered walls, booming sound system, and multi-room layout make it a mecca for headbangers and mosh pits. Every weekend, up-and-coming punk bands share the stage with established acts, ensuring that the spirit of rock stays alive in Nottingham.", 
                        ImageUrl = "nottinghamrockcity.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 57, 
                        Name = "Bristol Thekla", 
                        About = "Possibly the UK’s most unique music venue, Thekla is a repurposed cargo ship that has floated in Bristol Harbour since 1984. The venue’s industrial metal interiors, low ceilings, and multi-level standing areas create a truly immersive experience. Known for its alternative club nights, drum & bass sets, and indie band showcases, it attracts music lovers who crave something out of the ordinary.", 
                        ImageUrl = "bristolthekla.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 58, 
                        Name = "Brighton Concorde 2", 
                        About = "Sitting on Brighton Beach, Concorde 2 is a seaside club with a reputation for legendary electronic music nights. Originally a Victorian tea room, it was transformed into a club venue in the 1990s, hosting house DJs, drum & bass artists, and live indie acts. With its arched windows looking out onto the ocean, a minimalist dance floor, and a powerful sound system, it’s a favorite for those who love coastal nightlife with a bit of history.", 
                        ImageUrl = "brightonconcorde2.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 59, 
                        Name = "Cardiff Tramshed", 
                        About = "A former tram depot, the Tramshed is now one of Cardiff’s most dynamic music and arts venues. With its brick walls, warehouse-style open space, and high industrial ceilings, it caters to indie, hip-hop, and electronic artists alike. A favorite for both emerging and established acts, it combines urban grit with a creative artsy feel, making every gig feel raw and spontaneous.", 
                        ImageUrl = "cardifftramshed.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 60, 
                        Name = "Newcastle O2 Academy", 
                        About = "Originally a cinema built in 1927, the Newcastle O2 Academy still carries the grandeur of its past. Though it has been converted into a live music venue, it retains its ornate ceilings, sloped viewing area, and a grand yet intimate atmosphere. Hosting everything from rock gigs to hip-hop nights, it’s a staple of Newcastle’s music scene, where every show feels like a special event.",
                        ImageUrl = "newcastleo2.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 61, 
                        Name = "Oxford O2 Academy", 
                        About = "Tucked into the heart of Oxford, the O2 Academy has been a hotspot for indie and alternative music since the early 2000s. The venue’s no-frills design, intimate layout, and pulsating energy make it a favorite for student crowds and local music lovers. Whether it’s a high-energy rock show or an intimate acoustic gig, the Academy delivers pure live music energy.",
                        ImageUrl = "oxfordo2.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 62, 
                        Name = "Cambridge Corn Exchange", 
                        About = "Built in 1875, the Corn Exchange was once a trading hub for local merchants. Today, it’s a thriving concert hall known for its versatile performances—from classical concerts to indie rock gigs. The venue’s high ceilings, grand interior, and historic brickwork give it a regal feel, yet it maintains a cozy atmosphere that makes every show feel special.", 
                        ImageUrl = "cambridgecornexchange.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 63, 
                        Name = "Bath Komedia", 
                        About = "Originally a cinema in the 1920s, Komedia is now one of Bath’s most beloved arts venues, hosting live music, stand-up comedy, and indie performances. Its retro Art Deco aesthetic, dim red lighting, and intimate tables create a cabaret-style setting, perfect for a laid-back yet lively night out.", 
                        ImageUrl = "bathkomedia.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 64, 
                        Name = "Aberdeen The Lemon Tree", 
                        About = "Nestled in Aberdeen’s city center, The Lemon Tree has been a cultural hub for arts and music since the 1990s. Originally a warehouse space, it was converted into a small live venue that quickly became a favorite for indie bands, folk musicians, and spoken word artists. With its low ceilings, exposed brick walls, and intimate candlelit tables, it’s a venue where audiences are up close and personal with the performers.", 
                        ImageUrl = "aberdeenlemontree.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 65, 
                        Name = "York Barbican", 
                        About = "First opening in 1991, the York Barbican was built as a multi-purpose events hall. While it primarily hosts comedy, jazz, and classical performances, it also brings in rock, indie, and folk acts. Its modern yet intimate design—with a sloped viewing area, black-painted interiors, and simple stage lighting—makes it an inviting venue for both seated and standing audiences.", 
                        ImageUrl = "yorkbarbican.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 66, 
                        Name = "Belfast Limelight", 
                        About = "A true staple of Belfast’s alternative music scene, the Limelight has been pumping out rock, punk, and indie gigs since the 1980s. The venue consists of multiple rooms, including a smaller, grungy dive bar stage and a slightly larger gig room with low ceilings, neon bar signs, and black-painted walls. It’s the kind of place where up-and-coming bands cut their teeth before making it big.", 
                        ImageUrl = "belfastlimelight.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 67, 
                        Name = "Dublin Vicar Street", 
                        About = "Since opening in 1998, Vicar Street has built a reputation as Dublin’s most beloved live performance space. It’s known for hosting intimate gigs with world-famous artists, thanks to its cozy yet elegant layout. With wood-paneled walls, dim hanging lights, and a large but intimate standing area, it’s a venue that feels high-class yet unpretentious, making it the perfect stage for indie rock bands, singer-songwriters, and jazz ensembles.", 
                        ImageUrl = "dublinvicarstreet.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 68,
                        Name = "Norwich Waterfront", 
                        About = "Once a warehouse in the city’s docklands, the Waterfront became a music venue in the early 1990s, quickly growing into Norwich’s go-to spot for indie, punk, and alternative rock. The venue is dark and atmospheric, with a balcony area overlooking the stage, a black-painted ceiling, and gig posters covering the walls. The raw industrial feel gives it underground credibility, making it one of the best intimate venues in the UK.", 
                        ImageUrl = "norwichwaterfront.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 69, 
                        Name = "Exeter Phoenix", 
                        About = "Originally an arts and community center, the Phoenix is now Exeter’s leading multi-arts venue, known for its experimental theatre, visual art installations, and intimate music performances. The venue maintains its indie charm, with a small wooden stage, fairy lights hanging from the ceiling, and colorful murals decorating the walls. It’s the perfect home for emerging folk musicians, indie artists, and spoken word performers.", 
                        ImageUrl = "exeterphoenix.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 70, 
                        Name = "Southampton Engine Rooms",
                        About = "A converted industrial unit, the Engine Rooms is one of Southampton’s leading live music spaces. With its concrete floors, neon strip lighting, and a large bar area, it has an urban warehouse feel that makes it ideal for electronic music nights, indie gigs, and alternative club events. The space is simple but effective—no fancy seating, just standing room and a powerful sound system.", 
                        ImageUrl = "southamptonengine.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 71, 
                        Name = "Hull The Welly Club", 
                        About = "The Welly Club has been Hull’s favorite indie and rock venue since the 1980s. Its quirky name comes from its original life as a social club for working-class locals before it became a hotspot for Britpop bands and underground punk acts. It’s gritty, low-lit, and filled with a mix of dedicated gig-goers and students looking for a great night out.",
                        ImageUrl = "hullwellyclub.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 72, 
                        Name = "Plymouth Junction", 
                        About = "Once an old railway building, the Junction became Plymouth’s leading grassroots music venue in the early 2000s. Specializing in punk, metal, and alternative gigs, it has an underground, rebellious feel, with steel beams exposed, faded posters from past gigs, and a DIY stage that looks thrown together but delivers big energy.", 
                        ImageUrl = "plymouthjunction.jpg",
                        Approved = true },
                    new Venue { 
                        UserId = 73, 
                        Name = "Swansea Sin City", 
                        About = "A small but rowdy venue, Sin City is Swansea’s go-to spot for indie, rock, and alternative electronic music. With graffiti-covered walls, LED lighting, and a standing room-only layout, it delivers a chaotic but thrilling gig experience. The venue is infamous for sweaty mosh pits, wild DJ sets, and some of the best up-and-coming bands on tour.", 
                        ImageUrl = "swanseasincity.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 74, 
                        Name = "Inverness Ironworks", 
                        About = "The Ironworks is one of the Highlands' most important music venues, offering a rare space for touring bands in Scotland’s north. Built in an old industrial building, it blends rugged charm with professional staging, making it one of the most versatile venues in the country. Whether it’s a metal gig, a folk night, or a high-energy ceilidh, it provides a home for all genres and audiences.", 
                        ImageUrl = "invernessironworks.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 75, 
                        Name = "Stirling Albert Halls", 
                        About = "Dating back to the 1800s, the Albert Halls in Stirling originally hosted town meetings, orchestral performances, and dance nights. Now, it’s a stunning heritage venue used for folk music, jazz concerts, and intimate classical recitals. Its stained-glass windows, velvet-draped stage, and elegant chandeliers make it feel almost like a mini opera house, adding a touch of grandeur to every performance.", 
                        ImageUrl = "stirlingalberthalls.jpg", 
                        Approved = true },
                    new Venue { 
                        UserId = 76, 
                        Name = "Dundee Fat Sams", 
                        About = "A legendary rock club, Fat Sams has been Dundee’s number one live venue since the 1980s. It has an underground club feel, with a graffiti-covered entrance, blacked-out walls, and a huge dance floor where people cram together for the most raucous rock gigs. A favorite for late-night alternative music lovers, it’s a venue that never seems to sleep.", 
                        ImageUrl = "dundeefatsams.jpg", 
                        Approved = true },
                    new Venue {
                        UserId = 77, 
                        Name = "Coventry Empire", 
                        About = "Live music and club nights", 
                        ImageUrl = "coventryempire.jpg",
                        Approved = true }
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
                    new Listing { VenueId = 5, StartDate = new DateTime(2024, 4, 5, 20, 0, 0), EndDate = new DateTime(2024, 4, 5, 23, 0, 0), Pay = 275 },
                    new Listing { VenueId = 2, StartDate = new DateTime(2025, 5, 10, 20, 0, 0), EndDate = new DateTime(2025, 5, 10, 23, 0, 0), Pay = 300 },
                    new Listing { VenueId = 3, StartDate = new DateTime(2025, 1, 10, 18, 0, 0), EndDate = new DateTime(2024, 1, 10, 20, 0, 0), Pay = 150 },
                    new Listing { VenueId = 4, StartDate = new DateTime(2025, 6, 20, 18, 0, 0), EndDate = new DateTime(2025, 6, 20, 21, 0, 0), Pay = 250 },
                    new Listing { VenueId = 1, StartDate = new DateTime(2025, 3, 15, 19, 0, 0), EndDate = new DateTime(2024, 3, 15, 22, 0, 0), Pay = 200 },
                    new Listing { VenueId = 1, StartDate = new DateTime(2025, 5, 15, 19, 0, 0), EndDate = new DateTime(2024, 3, 15, 22, 0, 0), Pay = 250 },
                    new Listing { VenueId = 1, StartDate = new DateTime(2025, 5, 15, 19, 0, 0), EndDate = new DateTime(2024, 3, 15, 22, 0, 0), Pay = 250 },
                    new Listing { VenueId = 1, StartDate = new DateTime(2025, 6, 15, 19, 0, 0), EndDate = new DateTime(2024, 3, 15, 22, 0, 0), Pay = 250 },

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
