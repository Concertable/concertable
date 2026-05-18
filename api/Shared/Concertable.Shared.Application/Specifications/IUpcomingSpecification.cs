using Concertable.Shared;

namespace Concertable.Application.Interfaces.Specifications;

public interface IUpcomingSpecification<TEntity> where TEntity : class, IHasDateRange
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}
