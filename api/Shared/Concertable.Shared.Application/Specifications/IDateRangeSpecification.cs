using Concertable.Shared;

namespace Concertable.Application.Interfaces.Specifications;

public interface IDateRangeSpecification<TEntity> where TEntity : class, IHasDateRange
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, DateRange range);
}
