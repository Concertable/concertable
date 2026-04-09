using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Core.Entities;

public class VenueEntity : IIdEntity, IHasName, ILocatable<VenueEntity>
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public required string BannerUrl { get; set; }
    public bool Approved { get; set; }
    public VenueManagerEntity User { get; set; } = null!;
    public static Expression<Func<VenueEntity, Point?>> LocationExpression => v => v.User.Location;
    public ICollection<OpportunityEntity> Opportunities { get; set; } = [];
}
