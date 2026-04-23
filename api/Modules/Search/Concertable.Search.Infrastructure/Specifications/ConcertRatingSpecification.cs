using Concertable.Core.Projections;
using Concertable.Search.Application.Interfaces;

namespace Concertable.Search.Infrastructure.Specifications;

internal class ConcertRatingSpecification : IRatingSpecification<ConcertEntity>
{
    public IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews) =>
        reviews
            .GroupBy(r => r.Ticket.ConcertId)
            .Select(g => new RatingAggregate
            {
                EntityId = g.Key,
                AverageRating = Math.Round(g.Average(r => (double?)r.Stars) ?? 0.0, 1)
            });

    public IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id) =>
        reviews
            .Where(r => r.Ticket.ConcertId == id)
            .GroupBy(_ => 1)
            .Select(g => g.Average(r => (double?)r.Stars));
}
