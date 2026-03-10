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
    private readonly Func<Point, double, Expression<Func<TEntity, bool>>> locationFilter;

    public GeometrySpecification(
        IGeometryProvider geometryProvider,
        Func<Point, double, Expression<Func<TEntity, bool>>> locationFilter)
    {
        this.geometryProvider = geometryProvider;
        this.locationFilter = locationFilter;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams)
    {
        if (!searchParams.HasValidCoordinates())
            return query;

        var center = geometryProvider.CreatePoint(searchParams.Latitude!.Value, searchParams.Longitude!.Value);
        var radiusKm = searchParams.RadiusKm ?? 10;

        return query.Where(locationFilter(center, radiusKm));
    }
}
