using Application.Interfaces;
using Application.Interfaces.Rating;
using Application.Interfaces.Search;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Rating;

public class ArtistRatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext context;
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;
    private readonly IDapperRepository dapper;

    public ArtistRatingRepository(ApplicationDbContext context, IRatingSpecification<ArtistEntity> ratingSpecification, IDapperRepository dapper)
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
            SELECT ca.ArtistId AS Id, ROUND(AVG(CAST(r.Stars AS FLOAT)), 1) AS AvgRating
            FROM Reviews r
            JOIN Tickets t ON t.Id = r.TicketId
            JOIN Concerts c ON c.Id = t.ConcertId
            JOIN ConcertApplications ca ON ca.Id = c.ApplicationId
            WHERE ca.ArtistId IN @Ids
            GROUP BY ca.ArtistId", new { Ids = ids });

        return results.ToDictionary(x => x.Id, x => x.AvgRating);
    }
}
