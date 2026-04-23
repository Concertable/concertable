namespace Concertable.Search.Domain.Models;

public sealed class ConcertSearchModel
{
    public int Id { get; set; }
    public int ArtistId { get; set; }
    public ArtistSearchModel Artist { get; set; } = null!;
    public int VenueId { get; set; }
    public VenueSearchModel Venue { get; set; } = null!;
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public ICollection<ConcertSearchModelGenre> Genres { get; set; } = [];
}
