using NetTopologySuite.Geometries;

namespace Concertable.Concert.Domain;

public class ConcertEntity : IIdEntity, IHasName, IHasLocation
{
    public int Id { get; private set; }
    public int BookingId { get; private set; }
    public int ArtistId { get; private set; }
    public int VenueId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string? BannerUrl { get; private set; }
    public string? Avatar { get; private set; }
    public decimal Price { get; private set; }
    public int TotalTickets { get; private set; }
    public int AvailableTickets { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime? DatePosted { get; private set; }
    public Point? Location { get; set; }
    public BookingEntity Booking { get; set; } = null!;
    public ArtistReadModel Artist { get; set; } = null!;
    public VenueReadModel Venue { get; set; } = null!;
    public ICollection<TicketEntity> Tickets { get; } = [];
    public HashSet<ConcertGenreEntity> ConcertGenres { get; private set; } = [];
    public ICollection<ConcertImageEntity> Images { get; private set; } = [];

    private ConcertEntity() { }

    public static ConcertEntity CreateDraft(
        int bookingId,
        int artistId,
        int venueId,
        DateTime startDate,
        DateTime endDate,
        string name,
        string about,
        IEnumerable<int> genreIds) => new()
    {
        BookingId = bookingId,
        ArtistId = artistId,
        VenueId = venueId,
        StartDate = startDate,
        EndDate = endDate,
        Name = name,
        About = about,
        ConcertGenres = genreIds.Select(id => new ConcertGenreEntity { GenreId = id }).ToHashSet()
    };

    public void Update(string name, string about, decimal price, int totalTickets)
    {
        int ticketsSold = TotalTickets - AvailableTickets;
        Name = name;
        About = about;
        Price = price;
        TotalTickets = totalTickets;
        AvailableTickets = totalTickets - ticketsSold;
    }

    public void Post(string name, string about, decimal price, int totalTickets, DateTime now)
    {
        Name = name;
        About = about;
        Price = price;
        TotalTickets = totalTickets;
        AvailableTickets = totalTickets;
        DatePosted = now;
    }

    public void SellTickets(int quantity)
    {
        AvailableTickets -= quantity;
    }
}
