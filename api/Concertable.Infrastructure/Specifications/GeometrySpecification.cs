using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Interfaces.Search;
using Concertable.Infrastructure.Expressions;
using Concertable.Infrastructure.Services.Geometry;
using Microsoft.Extensions.DependencyInjection;
using Concertable.Core.Extensions;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Specifications;

public class GeometrySpecification<TEntity> : IGeometrySpecification<TEntity>
    where TEntity : class, IHasLocation
{
    private readonly IGeometryProvider geometryProvider;
    private readonly Expression<Func<TEntity, Point?>> locationSelector;

    public GeometrySpecification(
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        ILocationSelector<TEntity> locationSelector)
    {
        this.geometryProvider = geometryProvider;
        this.locationSelector = locationSelector.LocationSelector;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams geoParams)
    {
        if (!geoParams.HasValidCoordinates())
            return query;

        var center = geometryProvider.CreatePoint(geoParams.Latitude, geoParams.Longitude);
        if (center is null)
            return query;

        var radiusMeters = (geoParams.RadiusKm ?? 10) * 1000;

        return query.Where(BuildFilter(locationSelector, center, radiusMeters));
    }

    private static Expression<Func<TEntity, bool>> BuildFilter(
        Expression<Func<TEntity, Point?>> locationSelector,
        Point center,
        double radiusMeters)
    {
        Expression<Func<Point?, bool>> pointCondition =
            p => p != null && p.Distance(center) <= radiusMeters;

        var condition = new ParameterReplacer(pointCondition.Parameters.Single(), locationSelector.Body)
            .Visit(pointCondition.Body)!;

        return Expression.Lambda<Func<TEntity, bool>>(condition, locationSelector.Parameters.Single());
    }
}
