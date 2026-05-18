using Concertable.Application.Interfaces.Specifications;
using Concertable.Shared;

namespace Concertable.Shared.Infrastructure.Specifications;

internal class DateRangeSpecification<TEntity> : IDateRangeSpecification<TEntity>
    where TEntity : class, IHasDateRange
{
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, DateRange range)
        => query.Where(e => e.Period.Start < range.End && e.Period.End > range.Start);
}
