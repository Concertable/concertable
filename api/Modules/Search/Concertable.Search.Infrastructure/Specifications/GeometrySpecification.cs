using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Expressions;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Search.Application;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Search.Infrastructure.Specifications;

internal class GeometrySpecification<TEntity> : IGeometrySpecification<TEntity>
    where TEntity : class, IIdEntity, ILocatable<TEntity>
{
    private readonly IGeometryProvider geometryProvider;

    public GeometrySpecification(
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider)
    {
        this.geometryProvider = geometryProvider;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams geoParams)
    {
        if (!geoParams.HasValidCoordinates())
            return query;

        var center = geometryProvider.CreatePoint(geoParams.Latitude, geoParams.Longitude);
        if (center is null)
            return query;

        var radiusMeters = (geoParams.RadiusKm ?? 10) * 1000;

        Expression<Func<Point?, bool>> condition = p => p != null && p.Distance(center) <= radiusMeters;

        return query.Where(TEntity.LocationExpression.Substitute(condition));
    }
}
