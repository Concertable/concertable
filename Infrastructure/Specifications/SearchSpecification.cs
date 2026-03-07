using Application.Interfaces.Search;
using Core.Interfaces;
using Core.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Specifications
{
    public class SearchSpecification<TEntity> : ISearchSpecification<TEntity>
        where TEntity : class, ILocation
    {
        private readonly IGeometrySpecification<TEntity> geometrySpecification;

        public SearchSpecification(IGeometrySpecification<TEntity> geometrySpecification)
        {
            this.geometrySpecification = geometrySpecification;
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams)
        {
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
                query = query.Where(e => EF.Property<string>(e, "Name").Contains(searchParams.SearchTerm));

            query = geometrySpecification.Apply(query, searchParams);

            return query;
        }
    }
}
