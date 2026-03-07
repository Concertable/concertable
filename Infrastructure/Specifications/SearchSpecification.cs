using Application.Interfaces;
using Application.Interfaces.Search;
using Core.Parameters;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Infrastructure.Specifications
{
    public class SearchSpecification<TEntity> : ISearchSpecification<TEntity> where TEntity : class
    {
        private readonly IGeometryProvider geometryService;
        private readonly Func<Point, double, Expression<Func<TEntity, bool>>> geoFilter;

        public SearchSpecification(IGeometryProvider geometryService, Func<Point, double, Expression<Func<TEntity, bool>>> geoFilter)
        {
            this.geometryService = geometryService;
            this.geoFilter = geoFilter;
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
                query = query.Where(e => EF.Property<string>(e, "Name").Contains(searchParams.SearchTerm));

            if (GeoHelper.HasValidCoordinates(searchParams))
            {
                var center = geometryService.CreatePoint(searchParams.Latitude!.Value, searchParams.Longitude!.Value);
                var radiusKm = searchParams.RadiusKm ?? 10;
                query = query.Where(geoFilter(center, radiusKm));
            }

            return query;
        }
    }
}
