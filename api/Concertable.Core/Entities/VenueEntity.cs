using Core.Entities.Interfaces;
using Core.Interfaces;
using NetTopologySuite.Geometries;

namespace Core.Entities;

public class VenueEntity : IEntity, IHasName, IHasLocation
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string ImageUrl { get; set; }
    public bool Approved { get; set; }
    public VenueManagerEntity User { get; set; } = null!;
    public Point? Location => User.Location;
    public ICollection<ConcertOpportunityEntity> Opportunities { get; set; } = [];
}
