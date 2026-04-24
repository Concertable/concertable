using Concertable.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class OpportunityApplicationRepository : IdModuleRepository<OpportunityApplicationEntity, ConcertDbContext>, IOpportunityApplicationRepository
{
    private readonly TimeProvider timeProvider;

    public OpportunityApplicationRepository(ConcertDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<OpportunityApplicationEntity>> GetByOpportunityIdAsync(int id)
    {
        return await context.OpportunityApplications
            .Where(ca => ca.OpportunityId == id)
            .Include(ca => ca.Artist)
                .ThenInclude(a => a.Genres)
                    .ThenInclude(g => g.Genre)
            .Include(ca => ca.Opportunity)
                .ThenInclude(o => o.OpportunityGenres)
                    .ThenInclude(og => og.Genre)
            .ToListAsync();
    }

    public async Task<IEnumerable<OpportunityApplicationEntity>> GetPendingByArtistIdAsync(int artistId)
    {
        return await context.OpportunityApplications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.Venue)
            .Where(a =>
                a.ArtistId == artistId &&
                !context.ConcertBookings.Any(b => b.ApplicationId == a.Id) &&
                a.Opportunity.Period.Start > timeProvider.GetUtcNow())
            .ToListAsync();
    }

    public async Task<(ArtistReadModel, VenueReadModel)?> GetArtistAndVenueByIdAsync(int id)
    {
        var query = await context.OpportunityApplications
            .Where(ca => ca.Id == id)
            .Include(ca => ca.Artist)
            .Include(ca => ca.Opportunity)
                .ThenInclude(o => o.Venue)
            .FirstOrDefaultAsync();

        if (query is null) return null;
        return (query.Artist, query.Opportunity.Venue);
    }

    public async new Task<OpportunityApplicationEntity?> GetByIdAsync(int id)
    {
        return await context.OpportunityApplications
            .Where(ca => ca.Id == id)
            .Include(ca => ca.Artist)
                .ThenInclude(a => a.Genres)
                    .ThenInclude(g => g.Genre)
            .Include(ca => ca.Opportunity)
                .ThenInclude(o => o.OpportunityGenres)
                    .ThenInclude(og => og.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task RejectAllExceptAsync(int opportunityId, int applicationId)
    {
        await context.OpportunityApplications
            .Where(a => a.OpportunityId == opportunityId && a.Id != applicationId && a.Status == ApplicationStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, ApplicationStatus.Rejected));
    }

    public async Task<IEnumerable<OpportunityApplicationEntity>> GetRecentDeniedByArtistIdAsync(int artistId)
    {
        return await context.OpportunityApplications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.Venue)
            .Where(a =>
                a.ArtistId == artistId &&
                context.ConcertBookings.Any(b =>
                    b.Application.OpportunityId == a.OpportunityId &&
                    b.ApplicationId != a.Id))
            .OrderByDescending(a => a.Opportunity.Period.End)
            .Take(5)
            .ToListAsync();
    }
}
