using Application.Interfaces.Search;
using Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ArtistLocationSelector : ILocationSelector<ArtistEntity>
{
    public Expression<Func<ArtistEntity, Point?>> LocationSelector => a => a.User.Location;
}
