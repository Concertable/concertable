namespace Concertable.Search.Application.Interfaces;

public interface ISearchSpecification<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, string? searchTerm);
}
