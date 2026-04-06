using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Search;

public interface IRatingSpecification<TEntity>
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
