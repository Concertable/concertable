using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces.Search;

public interface ISortSpecification<T>
{
    IQueryable<T> Apply(IQueryable<T> query, ISortParams sortParams);
}
