using Core.Entities;
using Core.Entities.Identity;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ApplicationDbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            await context.Database.MigrateAsync(); //Creates db if not created yet

            System.Diagnostics.Debug.WriteLine("test");

            //Users
            if (!context.Users.Any())
            {
                var users = new ApplicationUser[]
                {
                    new Admin
                    {
                        UserName = "admin@test.com",
                        Email = "admin@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new Customer
                    {
                        UserName = "customer@test.com",
                        Email = "customer@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new ArtistManager
                    {
                        UserName = "artistmanager@test.com",
                        Email = "artistmanager@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager@test.com",
                        Email = "venuemanager@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new Customer
                    {
                        UserName = "customer2@test.com",
                        Email = "customer2@test.com",
                        County = "Surrey",
                        Town = "Ashtead"
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager2@test.com",
                        Email = "venuemanager2@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager3@test.com",
                        Email = "venuemanager3@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager4@test.com",
                        Email = "venuemanager4@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager5@test.com",
                        Email = "venuemanager5@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager6@test.com",
                        Email = "venuemanager6@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager7@test.com",
                        Email = "venuemanager7@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                    new VenueManager
                    {
                        UserName = "venuemanager8@test.com",
                        Email = "venuemanager8@test.com",
                        County = "Surrey",
                        Town = "Woking",
                    },
                };
                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Password11!");
                }
                await userManager.AddToRoleAsync(users[0], "Admin");
                await userManager.AddToRoleAsync(users[1], "Customer");
                await userManager.AddToRoleAsync(users[2], "ArtistManager");
                await userManager.AddToRoleAsync(users[3], "VenueManager");
                await userManager.AddToRoleAsync(users[4], "Customer");
                await userManager.AddToRoleAsync(users[5], "VenueManager");
                await userManager.AddToRoleAsync(users[6], "VenueManager");
                await userManager.AddToRoleAsync(users[7], "VenueManager");
                await userManager.AddToRoleAsync(users[8], "VenueManager");
                await userManager.AddToRoleAsync(users[9], "VenueManager");
                await userManager.AddToRoleAsync(users[10], "VenueManager");

            }
            //Genres
            if (!context.Genres.Any())
            {
                var genres = new Genre[]
                {
                    new Genre { Name = "Rock" },
                    new Genre { Name = "Pop" },
                    new Genre { Name = "Indie" },
                    new Genre { Name = "Alternative" },
                    new Genre { Name = "Electronic" },
                    new Genre { Name = "Hip-Hop" },
                    new Genre { Name = "R&B" },
                    new Genre { Name = "Jazz" },
                    new Genre { Name = "Blues" },
                    new Genre { Name = "Reggae" },
                    new Genre { Name = "Country" },
                    new Genre { Name = "Folk" },
                    new Genre { Name = "Metal" },
                    new Genre { Name = "Punk" },
                    new Genre { Name = "Soul" },
                    new Genre { Name = "Classical" },
                    new Genre { Name = "House" },
                    new Genre { Name = "Techno" },
                    new Genre { Name = "Trance" },
                    new Genre { Name = "Drum and Bass" },
                    new Genre { Name = "Dubstep" },
                    new Genre { Name = "Afrobeats" },
                    new Genre { Name = "Latin" },
                    new Genre { Name = "Ska" },
                    new Genre { Name = "Gospel" },
                    new Genre { Name = "Funk" },
                    new Genre { Name = "K-Pop" },
                    new Genre { Name = "J-Pop" },
                    new Genre { Name = "Grime" },
                    new Genre { Name = "Garage" },
                    new Genre { Name = "Hardcore" },
                    new Genre { Name = "EDM" },
                    new Genre { Name = "Synthwave" },
                    new Genre { Name = "Acoustic" },
                    new Genre { Name = "Lo-Fi" }
                };

                context.Genres.AddRange(genres);
                await context.SaveChangesAsync();
            }
            //Artists
            if (!context.Artists.Any())
            {
                var artists = new Artist[]
                {
                    new Artist
                    {
                        UserId = 3,
                        Name = "The Testys",
                        About = "We are a Rock Band!",
                        ImageUrl = "assets/images/test.jpg"
                    },
                };
                context.Artists.AddRange(artists);
                await context.SaveChangesAsync();
            }
            //ArtistGenres
            if(!context.ArtistGenres.Any())
            {
                var artistGenres = new ArtistGenre[]
                {
                    new ArtistGenre
                    {
                        ArtistId = 1,
                        GenreId = 1,
                    },
                    new ArtistGenre
                    {
                        ArtistId = 1,
                        GenreId = 2,
                    }
                };
                context.ArtistGenres.AddRange(artistGenres);
                await context.SaveChangesAsync();
            }
            //Venue
            if (!context.Listings.Any())
            {
                var venues = new Venue[]
                {
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 1",
                        About = "Test Venue 1",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 7,
                        Name = "The Test Venue 3",
                        About = "Test Venue 3",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 8,
                        Name = "The Test Venue 4",
                        About = "Test Venue 4",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 9,
                        Name = "The Test Venue 5",
                        About = "Test Venue 5",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 10,
                        Name = "The Test Venue 6",
                        About = "Test Venue 6",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 11,
                        Name = "The Test Venue 7",
                        About = "Test Venue 7",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 12,
                        Name = "The Test Venue 8",
                        About = "Test Venue 8",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        Approved = true
                    },
            };
                context.Venues.AddRange(venues);
                await context.SaveChangesAsync();
            }
            //Listings
            if (!context.Listings.Any())
            {
                var listings = new Listing[]
                {
                    new Listing
                    {
                        VenueId = 1,
                        StartDate = new DateTime(2024, 1, 23, 18, 00, 0),
                        EndDate = new DateTime(2024, 1, 23, 20, 00, 0),
                        Pay = 250
                    },
                    new Listing
                    {
                        VenueId = 1,
                        StartDate = new DateTime(2025, 3, 10, 20, 30, 0),
                        EndDate = new DateTime(2025, 3, 10, 20, 30, 0),
                        Pay = 100

                    },
                    new Listing
                    {
                        VenueId = 1,
                        StartDate = new DateTime(2025, 4, 5, 20, 30, 0),
                        EndDate = new DateTime(2025, 4, 5, 23, 30, 0),
                        Pay = 300
                    },
                };
                context.Listings.AddRange(listings);
                await context.SaveChangesAsync();
            }
            //ListingGenres
            if(!context.ListingGenres.Any())
            {
                var listingGenres = new ListingGenre[]
                {
                    new ListingGenre
                    {
                        ListingId = 1,
                        GenreId = 1,
                    },
                    new ListingGenre
                    {
                        ListingId = 1,
                        GenreId = 3,
                    },
                    new ListingGenre
                    {
                        ListingId = 2,
                        GenreId = 2,
                    },
                    new ListingGenre
                    {
                        ListingId = 2,
                        GenreId = 3,
                    },
                    new ListingGenre
                    {
                        ListingId = 3,
                        GenreId = 1,
                    },
                    new ListingGenre
                    {
                        ListingId = 3,
                        GenreId = 3,
                    },
                };
                context.ListingGenres.AddRange(listingGenres);
                await context.SaveChangesAsync();
            }
            //Registers
            if(!context.ListingApplications.Any())
            {
                var registers = new ListingApplication[]
                {
                    new ListingApplication
                    {
                        ArtistId = 1,
                        ListingId = 1
                    },
                    new ListingApplication
                    {
                        ArtistId = 1,
                        ListingId = 2
                    }
                };
                context.ListingApplications.AddRange(registers);
                await context.SaveChangesAsync();
            }
            //Events
            if (!context.Events.Any())
            {
                var events = new Event[]
                {
                    new Event
                    {
                        ApplicationId = 1,
                        Price = 10.5,
                        Name = "Test Event",
                        TotalTickets = 100,
                        AvailableTickets = 50
                    },
                    new Event
                    {
                        ApplicationId = 2,
                        Price = 10.5,
                        Name = "Test Event",
                        TotalTickets = 100,
                        AvailableTickets = 50
                    },
                };
                context.Events.AddRange(events);
                await context.SaveChangesAsync();
            }
            //Tickets
            if (!context.Tickets.Any())
            {
                var tickets = new Ticket[]
                 {
                    new Ticket
                    {
                        UserId = 2,
                        EventId = 1,
                        PurchaseDate = DateTime.Now

                    },
                    new Ticket
                    {
                        UserId = 5,
                        EventId = 1,
                        PurchaseDate = DateTime.Now
                    },
                 };
                context.Tickets.AddRange(tickets);
                await context.SaveChangesAsync();
            }
            //Reviews
            if (!context.Users.Any())
            {
                var reviews = new Review[]
                {
                    new Review
                    {
                        TicketId = 1,
                        Stars = 4,
                        Details = "Test"
                    },
                };
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }
        }
    }
}
