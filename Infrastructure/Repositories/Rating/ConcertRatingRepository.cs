using Application.Interfaces;
using Application.Interfaces.Rating;
using Application.Interfaces.Search;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Rating;

public class ConcertRatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext context;
    private readonly IRatingSpecification<ConcertEntity> ratingSpecification;

    public ConcertRatingRepository(ApplicationDbContext context, IRatingSpecification<ConcertEntity> ratingSpecification)
    {
        this.context = context;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<double> GetRatingAsync(int id)
    {
        return await ratingSpecification.ApplyAverage(context.Reviews, id).FirstOrDefaultAsync();
    }

    public async Task<IDictionary<int, double>> GetRatingsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any())
            return new Dictionary<int, double>();

        return await context.Reviews
            .Where(r => ids.Contains(r.Ticket.ConcertId))
            .GroupBy(r => r.Ticket.ConcertId)
            .Select(g => new
            {
                Id = g.Key,
                AvgRating = g.Average(r => (double?)r.Stars) ?? 0.0
            })
            .ToDictionaryAsync(
                x => x.Id,
                x => Math.Round(x.AvgRating, 1));
    }
}
