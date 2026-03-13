

using Core.Entities;

namespace Core.Entities;

public class Genre : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<ConcertGenre> ConcertGenres { get; } = [];
    public ICollection<OpportunityGenre> OpportunityGenres { get; set; } = [];
    public ICollection<ArtistGenre> ArtistGenres { get; set; } = [];
}
