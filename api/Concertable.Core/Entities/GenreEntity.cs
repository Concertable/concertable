using System.ComponentModel.DataAnnotations.Schema;

namespace Concertable.Core.Entities;

[Table("Genres")]
public class GenreEntity : IIdEntity
{
    public int Id { get; private set; }
    public required string Name { get; set; }
    public ICollection<ConcertGenreEntity> ConcertGenres { get; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
    public ICollection<ArtistGenreEntity> ArtistGenres { get; set; } = [];

    public static GenreEntity Create(int id, string name) => new() { Id = id, Name = name };
}
