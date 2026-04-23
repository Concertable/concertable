namespace Concertable.Search.Domain.Models;

public sealed class ArtistSearchModelGenre
{
    public int ArtistSearchModelId { get; set; }
    public int GenreId { get; set; }
    public ArtistSearchModel Artist { get; set; } = null!;
}
