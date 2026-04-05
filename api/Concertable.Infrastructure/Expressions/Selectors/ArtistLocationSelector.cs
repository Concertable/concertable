using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Expressions.Selectors;

public class ArtistLocationSelector : ILocationSelector<ArtistEntity>
{
    public Expression<Func<ArtistEntity, Point?>> LocationSelector => a => a.User.Location;
}
