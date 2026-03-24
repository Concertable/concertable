using Application.Interfaces.Search;
using Core.Entities.Interfaces;
using Core.Interfaces;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class SearchSpecification<TEntity> : ISearchSpecification<TEntity>
    where TEntity : IEntity, IHasName
{
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams)
    {
        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            query = query.Where(e => e.Name.Contains(searchParams.SearchTerm));

        return searchParams.Sort?.ToLower() switch
        {
            "name_asc" => query.OrderBy(a => a.Name),
            "name_desc" => query.OrderByDescending(a => a.Name),
            _ => query.OrderBy(a => a.Id)
        };
    }
}
