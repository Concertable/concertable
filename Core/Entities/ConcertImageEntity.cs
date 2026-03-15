

namespace Core.Entities;

public class ConcertImageEntity : BaseEntity
{
    public int ConcertId { get; set; }
    public int Type { get; set; }
    public required string Url { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
}
