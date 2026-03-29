using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Specifications;

public class VenueLocationSelector : ILocationSelector<VenueEntity>
{
    public Expression<Func<VenueEntity, Point?>> LocationSelector => v => v.User.Location;
}
