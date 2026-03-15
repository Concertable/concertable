

using Core.Entities;

namespace Core.Entities;

public class GenreEntity : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<ConcertGenreEntity> ConcertGenres { get; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
    public ICollection<ArtistGenreEntity> ArtistGenres { get; set; } = [];
}
