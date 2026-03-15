using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey(nameof(ConcertId), nameof(GenreId))]
public class ConcertGenreEntity
{
    public int ConcertId { get; set; }
    public int GenreId { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
