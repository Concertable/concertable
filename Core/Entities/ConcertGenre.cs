using Microsoft.EntityFrameworkCore;

namespace Core.Entities
{
    [PrimaryKey(nameof(ConcertId), nameof(GenreId))]
    public class ConcertGenre
    {
        public int ConcertId { get; set; }
        public int GenreId { get; set; }
        public Concert Concert { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}
