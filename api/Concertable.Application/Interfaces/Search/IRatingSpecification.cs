using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Core.Projections;

namespace Concertable.Application.Interfaces.Search;

public interface IRatingSpecification<TEntity> where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
