using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class GenreEntity : IEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<ConcertGenreEntity> ConcertGenres { get; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
    public ICollection<ArtistGenreEntity> ArtistGenres { get; set; } = [];
}
