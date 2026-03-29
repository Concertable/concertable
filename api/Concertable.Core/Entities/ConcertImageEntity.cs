using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class ConcertImageEntity : IEntity
{
    public int Id { get; set; }
    public int ConcertId { get; set; }
    public int Type { get; set; }
    public required string Url { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
}
