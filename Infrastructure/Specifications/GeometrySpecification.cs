using Application.Interfaces;
using Application.Interfaces.Search;
using Core.Extensions;
using Core.Interfaces;
using Core.Parameters;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class GeometrySpecification<TEntity> : IGeometrySpecification<TEntity>
    where TEntity : class, IHasLocation
{
    private readonly IGeometryProvider geometryProvider;
    private readonly ILocationSelector<TEntity> locationSelector;

    public GeometrySpecification(
        IGeometryProvider geometryProvider,
        ILocationSelector<TEntity> locationSelector)
    {
        this.geometryProvider = geometryProvider;
        this.locationSelector = locationSelector;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams geoParams)
    {
        if (!geoParams.HasValidCoordinates())
            return query;

        var center = geometryProvider.CreatePoint(geoParams.Latitude!.Value, geoParams.Longitude!.Value);
        var radiusKm = geoParams.RadiusKm ?? 10;

        var entityParam = locationSelector.LocationSelector.Parameters[0];
        var locationExpr = locationSelector.LocationSelector.Body;

        /* e => e.[LocationPath] != null
               && e.[LocationPath].Distance(center) <= radiusKm * 1000 */
        var filter = Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(
                Expression.NotEqual(locationExpr, Expression.Constant(null, typeof(Point))),
                Expression.LessThanOrEqual(
                    Expression.Call(locationExpr, nameof(Geometry.Distance), null, Expression.Constant(center)),
                    Expression.Constant(radiusKm * 1000))),
            entityParam);

        return query.Where(filter);
    }
}
