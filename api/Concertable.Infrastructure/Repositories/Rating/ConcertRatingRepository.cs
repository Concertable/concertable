using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Rating;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Rating;

public class ConcertRatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext context;
    private readonly IRatingSpecification<ConcertEntity> ratingSpecification;
    private readonly IDapperRepository dapper;

    public ConcertRatingRepository(ApplicationDbContext context, IRatingSpecification<ConcertEntity> ratingSpecification, IDapperRepository dapper)
    {
        this.context = context;
        this.ratingSpecification = ratingSpecification;
        this.dapper = dapper;
    }

    public async Task<double> GetRatingAsync(int id)
    {
        var avg = await ratingSpecification.ApplyAverage(context.Reviews, id).FirstOrDefaultAsync();
        return avg ?? 0.0;
    }

    public async Task<IDictionary<int, double>> GetRatingsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any())
            return new Dictionary<int, double>();

        var results = await dapper.QueryAsync<(int Id, double AvgRating)>(@"
            SELECT t.ConcertId AS Id, ROUND(AVG(CAST(r.Stars AS FLOAT)), 1) AS AvgRating
            FROM Reviews r
            JOIN Tickets t ON t.Id = r.TicketId
            WHERE t.ConcertId IN @Ids
            GROUP BY t.ConcertId", new { Ids = ids });

        return results.ToDictionary(x => x.Id, x => x.AvgRating);
    }
}
