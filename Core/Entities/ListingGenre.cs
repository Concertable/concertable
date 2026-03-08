using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey(nameof(ListingId), nameof(GenreId))]
public class ListingGenre
{
    public int ListingId { get; set; }
    public int GenreId { get; set; }
    public Listing Listing { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
