

using Core.Entities;

namespace Core.Entities;

public class Genre : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<ConcertGenre> ConcertGenres { get; } = new List<ConcertGenre>();
    public ICollection<ListingGenre> ListingGenres { get; set; } = new List<ListingGenre>();
    public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
}
