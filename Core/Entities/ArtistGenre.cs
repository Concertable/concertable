using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey(nameof(ArtistId), nameof(GenreId))]
public class ArtistGenre
{
    public int ArtistId { get; set; }
    public int GenreId { get; set; }
    public Artist Artist { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
