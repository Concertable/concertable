namespace Concertable.Artist.Domain;

public class ArtistGenreEntity : IGenreJoin
{
    public int ArtistId { get; set; }
    public int GenreId { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
