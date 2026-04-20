namespace Concertable.Search.Application.Interfaces;

public interface ISortSpecification<T>
{
    IQueryable<T> Apply(IQueryable<T> query, ISortParams sortParams);
}
