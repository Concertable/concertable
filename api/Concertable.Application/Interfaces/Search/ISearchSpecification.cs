namespace Concertable.Application.Interfaces.Search;

public interface ISearchSpecification<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, string? searchTerm);
}
