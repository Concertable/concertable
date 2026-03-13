using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Specifications;

public class RatingSpecification<TEntity> : IRatingSpecification<TEntity>
{
    private readonly IReviewKeySelector<TEntity> keySelector;

    public RatingSpecification(IReviewKeySelector<TEntity> keySelector)
    {
        this.keySelector = keySelector;
    }

    public IQueryable<RatingResult> Apply(IQueryable<Review> reviews) =>
        reviews
            .GroupBy(keySelector.KeySelector)
            .Select(g => new RatingResult
            {
                EntityId = g.Key,
                AverageRating = Math.Round(g.Average(r => (double?)r.Stars) ?? 0.0, 1)
            });
}
