using Core.Parameters;

namespace Application.Interfaces.Search;

public interface ISearchSpecification<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams);
}
