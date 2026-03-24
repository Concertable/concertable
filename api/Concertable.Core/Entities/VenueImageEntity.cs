using Core.Entities.Interfaces;

namespace Core.Entities;

public class VenueImageEntity : IEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public required string Url { get; set; }
    public VenueEntity Venue { get; set; } = null!;
}
