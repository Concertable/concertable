using Application.Interfaces.Search;
using Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ConcertLocationSelector : ILocationSelector<Concert>
{
    public Expression<Func<Concert, Point?>> LocationSelector => c => c.Application.Opportunity.Venue.User.Location;
}
