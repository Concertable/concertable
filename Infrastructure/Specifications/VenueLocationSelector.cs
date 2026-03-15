using Application.Interfaces.Search;
using Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class VenueLocationSelector : ILocationSelector<VenueEntity>
{
    public Expression<Func<VenueEntity, Point?>> LocationSelector => v => v.User.Location;
}
