using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Expressions.Selectors;

public class ConcertLocationSelector : ILocationSelector<ConcertEntity>
{
    public Expression<Func<ConcertEntity, Point?>> LocationSelector => c => c.Application.Opportunity.Venue.User.Location;
}
