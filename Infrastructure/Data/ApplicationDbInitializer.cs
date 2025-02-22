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
                    new Admin { UserName = "admin1@test.com", Email = "admin1@test.com", County = "Surrey", Town = "Woking", Latitude = 51.0, Longitude = -0.5 },
                    new Customer { UserName = "customer1@test.com", Email = "customer1@test.com", County = "Surrey", Town = "Guildford", Latitude = 51.25, Longitude = -0.56 },
                    new Customer { UserName = "customer2@test.com", Email = "customer2@test.com", County = "Surrey", Town = "Epsom", Latitude = 51.34, Longitude = -0.27 },
                    // Artist Managers
                    new ArtistManager { UserName = "artistmanager1@test.com", Email = "artistmanager1@test.com", County = "Surrey", Town = "Dorking", Latitude = 51.23, Longitude = -0.33 },
                    new ArtistManager { UserName = "artistmanager2@test.com", Email = "artistmanager2@test.com", County = "Surrey", Town = "Reigate", Latitude = 51.23, Longitude = -0.17 },
                    new ArtistManager { UserName = "artistmanager3@test.com", Email = "artistmanager3@test.com", County = "Surrey", Town = "Farnham", Latitude = 51.21, Longitude = -0.58 },
                    new ArtistManager { UserName = "artistmanager4@test.com", Email = "artistmanager4@test.com", County = "Surrey", Town = "Camberley", Latitude = 51.34, Longitude = -0.73 },
                    new ArtistManager { UserName = "artistmanager5@test.com", Email = "artistmanager5@test.com", County = "Surrey", Town = "Haslemere", Latitude = 51.08, Longitude = -0.74 },
                    // Venue Managers
                    new VenueManager { UserName = "venuemanager1@test.com", Email = "venuemanager1@test.com", County = "Surrey", Town = "Leatherhead", Latitude = 51.3, Longitude = -0.3 },
                    new VenueManager { UserName = "venuemanager2@test.com", Email = "venuemanager2@test.com", County = "Surrey", Town = "Redhill", Latitude = 51.23, Longitude = -0.17 },
                    new VenueManager { UserName = "venuemanager3@test.com", Email = "venuemanager3@test.com", County = "Surrey", Town = "Weybridge", Latitude = 51.38, Longitude = -0.46 },
                    new VenueManager { UserName = "venuemanager4@test.com", Email = "venuemanager4@test.com", County = "Surrey", Town = "Cobham", Latitude = 51.32, Longitude = -0.46 },
                    new VenueManager { UserName = "venuemanager5@test.com", Email = "venuemanager5@test.com", County = "Surrey", Town = "Chertsey", Latitude = 51.39, Longitude = -0.5 }
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
                    new Artist { UserId = 4, Name = "The Rockers", About = "A thrilling rock band", ImageUrl = "rockers.jpg" },
                    new Artist { UserId = 5, Name = "Indie Vibes", About = "Smooth indie tunes", ImageUrl = "indievibes.jpg" },
                    new Artist { UserId = 6, Name = "Electronic Pulse", About = "Pumping electronic beats", ImageUrl = "electronicpulse.jpg" },
                    new Artist { UserId = 7, Name = "Hip-Hop Flow", About = "Smooth hip-hop beats", ImageUrl = "hiphopflow.jpg" },
                    new Artist { UserId = 8, Name = "Jazz Masters", About = "Timeless jazz performances", ImageUrl = "jazzmaster.jpg" }
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
                    new Venue { UserId = 9, Name = "The Grand Venue", About = "Premier event venue in Leatherhead", ImageUrl = "grandvenue.jpg", Approved = true },
                    new Venue {UserId = 10, Name = "Redhill Hall", About = "Historic hall for intimate gigs", ImageUrl = "redhillhall.jpg", Approved = true},
                    new Venue {UserId = 11, Name = "Weybridge Pavilion", About = "Modern space for concerts", ImageUrl = "weybridgepavilon.jpg", Approved = true},
                    new Venue {UserId = 12, Name = "Cobham Arts Centre", About = "Cultural hub for arts and music", ImageUrl = "cobhamarts.jpg", Approved = true},
                    new Venue {UserId = 13, Name = "Chertsey Arena", About = "Large arena for big events", ImageUrl = "chertseyarena.jpg", Approved = true}
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
