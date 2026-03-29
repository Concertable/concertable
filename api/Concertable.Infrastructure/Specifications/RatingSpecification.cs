using Concertable.Application.Interfaces.Search;
using Concertable.Core.Projections;
using Concertable.Core.Entities;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Specifications;

public class RatingSpecification<TEntity> : IRatingSpecification<TEntity>
{
    private readonly Expression<Func<ReviewEntity, int>> keySelector;

    public RatingSpecification(IReviewKeySelector<TEntity> keySelector)
    {
        this.keySelector = keySelector.KeySelector;
    }

    public IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews) =>
        reviews
            .GroupBy(keySelector)
            .Select(g => new RatingAggregate
            {
                EntityId = g.Key,
                AverageRating = Math.Round(g.Average(r => (double?)r.Stars) ?? 0.0, 1)
            });

    public IQueryable<double> ApplyAverage(IQueryable<ReviewEntity> reviews, int id)
    {
        var param = keySelector.Parameters[0];
        var filter = Expression.Lambda<Func<ReviewEntity, bool>>(
            Expression.Equal(keySelector.Body, Expression.Constant(id)),
            param);

        return reviews.Where(filter).GroupBy(_ => 0).Select(g => Math.Round(g.Average(r => (double?)r.Stars) ?? 0.0, 1));
    }
}
