using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Microsoft.EntityFrameworkCore;
using Concertable.Infrastructure.Data;

namespace Infrastructure.Repositories.Concert;

public class ConcertOpportunityRepository : Repository<ConcertOpportunityEntity>, IConcertOpportunityRepository
{
    private readonly TimeProvider timeProvider;

    public ConcertOpportunityRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<ConcertOpportunityEntity>> GetActiveByVenueIdAsync(int id)
    {
        var query = context.ConcertOpportunities
            .Where(o => o.VenueId == id && o.StartDate >= timeProvider.GetUtcNow())
            .Where(o => !context.Concerts.Any(e => e.ApplicationId ==
                context.ConcertApplications
                    .Where(ca => ca.OpportunityId == o.Id)
                    .Select(ca => ca.Id)
                    .FirstOrDefault()))
            .Include(o => o.OpportunityGenres)
            .ThenInclude(og => og.Genre)
            .OrderBy(o => o.StartDate);

        return await query.ToListAsync();
    }

    public async Task<UserEntity?> GetOwnerByIdAsync(int opportunityId)
    {
        return await context.Users
            .OfType<VenueManagerEntity>()
            .Where(vm => vm.Venue != null && vm.Venue.Opportunities.Any(o => o.Id == opportunityId))
            .FirstOrDefaultAsync();
    }

    public async new Task<ConcertOpportunityEntity?> GetByIdAsync(int id)
    {
        return await context.ConcertOpportunities
            .Where(o => o.Id == id)
            .Include(o => o.Contract)
            .Include(o => o.OpportunityGenres)
                .ThenInclude(og => og.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertOpportunityEntity?> GetWithVenueByIdAsync(int id)
    {
        return await context.ConcertOpportunities
            .Where(o => o.Id == id)
            .Include(o => o.Venue)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertOpportunityEntity?> GetByApplicationIdAsync(int id)
    {
        return await context.ConcertOpportunities
            .Include(o => o.OpportunityGenres)
                .ThenInclude(og => og.Genre)
            .Include(o => o.Venue)
                .ThenInclude(v => v.User)
            .Where(o => o.Applications.Any(a => a.Id == id))
            .FirstOrDefaultAsync();
    }
}
