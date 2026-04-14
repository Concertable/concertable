using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Concert;

public class OpportunityRepository : Repository<OpportunityEntity>, IOpportunityRepository
{
    private readonly TimeProvider timeProvider;

    public OpportunityRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IPagination<OpportunityEntity>> GetActiveByVenueIdAsync(int id, IPageParams pageParams)
    {
        var query = context.Opportunities
            .Where(o => o.VenueId == id && o.Period.Start >= timeProvider.GetUtcNow())
            .Where(o => !context.Concerts.Any(e => e.ApplicationId ==
                context.OpportunityApplications
                    .Where(ca => ca.OpportunityId == o.Id)
                    .Select(ca => ca.Id)
                    .FirstOrDefault()))
            .Include(o => o.Contract)
            .Include(o => o.OpportunityGenres)
            .ThenInclude(og => og.Genre)
            .OrderBy(o => o.Period.Start);

        return await query.ToPaginationAsync(pageParams);
    }

    public async Task<UserEntity?> GetOwnerByIdAsync(int opportunityId)
    {
        return await context.Users
            .OfType<VenueManagerEntity>()
            .Where(vm => vm.Venue != null && vm.Venue.Opportunities.Any(o => o.Id == opportunityId))
            .FirstOrDefaultAsync();
    }

    public async new Task<OpportunityEntity?> GetByIdAsync(int id)
    {
        return await context.Opportunities
            .Where(o => o.Id == id)
            .Include(o => o.Contract)
            .Include(o => o.OpportunityGenres)
                .ThenInclude(og => og.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<OpportunityEntity?> GetWithVenueByIdAsync(int id)
    {
        return await context.Opportunities
            .Where(o => o.Id == id)
            .Include(o => o.Venue)
            .FirstOrDefaultAsync();
    }

    public async Task<OpportunityEntity?> GetByApplicationIdAsync(int id)
    {
        return await context.Opportunities
            .Include(o => o.OpportunityGenres)
                .ThenInclude(og => og.Genre)
            .Include(o => o.Venue)
                .ThenInclude(v => v.User)
            .Where(o => o.Applications.Any(a => a.Id == id))
            .FirstOrDefaultAsync();
    }
}
