using Concertable.Shared;

namespace Concertable.Search.Domain.Models;

public sealed class ArtistSearchModelGenre
{
    public int ArtistId { get; set; }
    public int GenreId { get; set; }
    public ArtistSearchModel Artist { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
