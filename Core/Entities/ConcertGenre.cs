using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    [PrimaryKey(nameof(ConcertId), nameof(GenreId))]
    public class ConcertGenre
    {
        public int ConcertId { get; set; }
        public int GenreId { get; set; }
        public Concert Concert { get; set; }
        public Genre Genre { get; set; }
    }
}
