namespace Concertable.Concert.Domain;

public class ArtistReadModelGenre
{
    public int ArtistReadModelId { get; set; }
    public int GenreId { get; set; }
    public ArtistReadModel Artist { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
