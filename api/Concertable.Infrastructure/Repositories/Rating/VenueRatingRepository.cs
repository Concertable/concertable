using Application.Interfaces;
using Application.Interfaces.Rating;
using Application.Interfaces.Search;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Rating;

public class VenueRatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext context;
    private readonly IRatingSpecification<VenueEntity> ratingSpecification;
    private readonly IDapperRepository dapper;

    public VenueRatingRepository(ApplicationDbContext context, IRatingSpecification<VenueEntity> ratingSpecification, IDapperRepository dapper)
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
            SELECT co.VenueId AS Id, ROUND(AVG(CAST(r.Stars AS FLOAT)), 1) AS AvgRating
            FROM Reviews r
            JOIN Tickets t ON t.Id = r.TicketId
            JOIN Concerts c ON c.Id = t.ConcertId
            JOIN ConcertApplications ca ON ca.Id = c.ApplicationId
            JOIN ConcertOpportunities co ON co.Id = ca.OpportunityId
            WHERE co.VenueId IN @Ids
            GROUP BY co.VenueId", new { Ids = ids });

        return results.ToDictionary(x => x.Id, x => x.AvgRating);
    }
}
