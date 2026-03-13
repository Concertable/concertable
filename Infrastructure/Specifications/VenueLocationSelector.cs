using Application.Interfaces.Search;
using Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class VenueLocationSelector : ILocationSelector<Venue>
{
    public Expression<Func<Venue, Point?>> LocationSelector => v => v.User.Location;
}
