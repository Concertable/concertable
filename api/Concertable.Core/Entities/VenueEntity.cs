using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;

namespace Concertable.Core.Entities;

public class VenueEntity : IIdEntity, IHasName, IHasLocation
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string BannerUrl { get; set; }
    public bool Approved { get; set; }
    public VenueManagerEntity User { get; set; } = null!;
    public Point? Location => User.Location;
    public ICollection<OpportunityEntity> Opportunities { get; set; } = [];
}
