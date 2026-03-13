using Application.Interfaces.Search;
using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Specifications;

public class SearchSpecification<TEntity> : ISearchSpecification<TEntity>
    where TEntity : BaseEntity, IHasName, IHasLocation
{
    private readonly IGeometrySpecification<TEntity> geometrySpecification;

    public SearchSpecification(IGeometrySpecification<TEntity> geometrySpecification)
    {
        this.geometrySpecification = geometrySpecification;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams)
    {
        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            query = query.Where(e => e.Name.Contains(searchParams.SearchTerm));

        query = geometrySpecification.Apply(query, searchParams);

        return searchParams.Sort?.ToLower() switch
        {
            "name_asc" => query.OrderBy(a => a.Name),
            "name_desc" => query.OrderByDescending(a => a.Name),
            _ => query.OrderBy(a => a.Id)
        }; ;
    }
}
