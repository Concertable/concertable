

namespace Core.Entities;

public class VenueImageEntity : BaseEntity
{
    public int VenueId { get; set; }
    public required string Url { get; set; }
    public VenueEntity Venue { get; set; } = null!;
}
