using Application.Interfaces.Search;
using Core.Entities;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ArtistLocationSelector : ILocationSelector<Artist>
{
    public Expression<Func<Artist, Point?>> LocationSelector => a => a.User.Location;
}
