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
                    new ApplicationUser
                    {
                        UserName = "admin@test.com",
                        Email = "admin@test.com",
                    },
                    new ApplicationUser
                    {
                        UserName = "customer@test.com",
                        Email = "customer@test.com",
                    },
                    new ApplicationUser
                    {
                        UserName = "artistmanager@test.com",
                        Email = "artistmanager@test.com",
                    },
                    new ApplicationUser
                    {
                        UserName = "venuemanager@test.com",
                        Email = "venuemanager@test.com",
                    },
                    new ApplicationUser
                    {
                        UserName = "customer2@test.com",
                        Email = "customer2@test.com",
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
            }
            //Genres
            if (!context.Genres.Any())
            {
                var genres = new Genre[]
                {
                    new Genre
                    {
                        Name = "Rock",
                    },
                    new Genre
                    {
                        Name = "Pop",
                    },
                    new Genre
                    {
                        Name = "Indie",
                    },
                    new Genre
                    {
                        Name = "Alternative",
                    },
                    new Genre
                    {
                        Name = "Electric",
                    }
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
                        ImageUrl = ""
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
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 2",
                        About = "Test Venue 2",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 3",
                        About = "Test Venue 3",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 4",
                        About = "Test Venue 4",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 5",
                        About = "Test Venue 5",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 6",
                        About = "Test Venue 6",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 7",
                        About = "Test Venue 7",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 8",
                        About = "Test Venue 8",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 9",
                        About = "Test Venue 9",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 10",
                        About = "Test Venue 10",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 11",
                        About = "Test Venue 11",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
                        Approved = true
                    },
                    new Venue
                    {
                        UserId = 4,
                        Name = "The Test Venue 12",
                        About = "Test Venue 12",
                        Longitude = 0,
                        Latitude = 0,
                        ImageUrl = "assets/images/test.jpg",
                        County = "Surrey",
                        Town = "Woking",
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
                        About = "A Test Event",
                        TotalTickets = 100,
                        AvailableTickets = 50,
                        ImageUrl = ""
                    },
                    new Event
                    {
                        ApplicationId = 2,
                        Price = 10.5,
                        Name = "Test Event",
                        About = "A Test Event",
                        TotalTickets = 100,
                        AvailableTickets = 50,
                        ImageUrl = ""
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
