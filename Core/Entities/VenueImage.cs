

namespace Core.Entities;

public class VenueImage : BaseEntity
{
    public int VenueId { get; set; }
    public required string Url { get; set; }
    public Venue Venue { get; set; } = null!;
}
