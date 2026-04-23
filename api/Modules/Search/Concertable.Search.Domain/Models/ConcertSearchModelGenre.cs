namespace Concertable.Search.Domain.Models;

public sealed class ConcertSearchModelGenre
{
    public int ConcertSearchModelId { get; set; }
    public int GenreId { get; set; }
    public ConcertSearchModel Concert { get; set; } = null!;
}
