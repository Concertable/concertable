using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using Concertable.Core.Projections;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Expressions;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Specifications;

public class RatingSpecification<TEntity> : IRatingSpecification<TEntity>
    where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    public IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews) =>
        reviews
            .GroupBy(TEntity.ReviewIdSelector)
            .Select(g => new RatingAggregate
            {
                EntityId = g.Key,
                AverageRating = Math.Round(g.Average(r => (double?)r.Stars) ?? 0.0, 1)
            });

    public IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id)
    {
        Expression<Func<int, bool>> condition = reviewId => reviewId == id;
        return reviews
            .Where(TEntity.ReviewIdSelector.Substitute(condition))
            .GroupBy(_ => 1)
            .Select(g => g.Average(r => (double?)r.Stars));
    }
}
