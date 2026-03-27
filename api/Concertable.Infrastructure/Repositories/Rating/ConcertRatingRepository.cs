using Application.Interfaces;
using Application.Interfaces.Rating;
using Application.Interfaces.Search;
using Concertable.Infrastructure.Data;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Rating;

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
        return await ratingSpecification.ApplyAverage(context.Reviews, id).FirstOrDefaultAsync();
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
