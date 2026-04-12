using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;

namespace Concertable.Infrastructure.Specifications;

public class SearchSpecification<TEntity> : ISearchSpecification<TEntity>
    where TEntity : IEntity, IHasName
{
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(e => e.Name.Contains(searchTerm));

        return query;
    }
}
