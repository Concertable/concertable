using Concertable.Application.Interfaces.Specifications;
using Concertable.Shared;

namespace Concertable.Shared.Infrastructure.Specifications;

internal class UpcomingSpecification<TEntity> : IUpcomingSpecification<TEntity>
    where TEntity : class, IHasDateRange
{
    private readonly TimeProvider timeProvider;

    public UpcomingSpecification(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return query.Where(e => e.Period.End > now);
    }
}
