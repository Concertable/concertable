using Concertable.Shared;

namespace Concertable.Search.Domain.Models;

public sealed class ConcertSearchModelGenre
{
    public int ConcertId { get; set; }
    public int GenreId { get; set; }
    public ConcertSearchModel Concert { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
