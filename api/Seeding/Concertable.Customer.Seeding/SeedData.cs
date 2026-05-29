using Concertable.Contracts;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Kernel;
using Concertable.Seeding.Identity;

namespace Concertable.Customer.Seeding;

public sealed class SeedData
{
    public const string TestPassword = "Password11!";
    public const int UpcomingConcertId = 13;

    public SeedCustomer Customer { get; }
    public IReadOnlyList<Guid> CustomerIds { get; }

    public VenueReadModel Venue { get; }
    public ArtistReadModel Artist { get; }
    public ConcertReadModel UpcomingConcert { get; }

    public SeedData()
    {
        Customer = SeedCustomers.Customer1;
        CustomerIds = [.. SeedCustomers.All.Select(c => c.Id)];

        Venue = VenueReadModel.Create(
            venueId: 1,
            userId: Guid.Empty,
            name: "The Grand Venue",
            about: "Test venue",
            avatar: "avatar.jpg",
            bannerUrl: "grandvenue.jpg",
            county: "Test County",
            town: "Test Town",
            latitude: 51.0,
            longitude: 0.0,
            email: "thegrandvenue@test.com");

        Artist = ArtistReadModel.Create(
            artistId: 2,
            userId: Guid.Empty,
            name: "Indie Vibes",
            about: "Test artist",
            avatar: "avatar.jpg",
            bannerUrl: "indievibes.jpg",
            county: "Test County",
            town: "Test Town",
            latitude: 51.0,
            longitude: 0.0,
            email: "indievibes@test.com");

        var now = DateTime.UtcNow;
        UpcomingConcert = ConcertReadModel.Create(
            concertId: UpcomingConcertId,
            name: "Upcoming FlatFee Show",
            about: "Test concert",
            bannerUrl: null,
            avatar: null,
            totalTickets: 150,
            price: 20m,
            period: new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)),
            datePosted: now,
            artistId: Artist.Id,
            artistName: Artist.Name,
            venueId: Venue.Id,
            venueName: Venue.Name);
    }
}
