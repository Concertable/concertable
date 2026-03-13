using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ArtistRatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext context;

    public ArtistRatingRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<IDictionary<int, double>> GetRatingsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any())
            return new Dictionary<int, double>();

        return await context.Reviews
            .Where(r => ids.Contains(r.Ticket.Concert.Application.ArtistId))
            .GroupBy(r => r.Ticket.Concert.Application.ArtistId)
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
