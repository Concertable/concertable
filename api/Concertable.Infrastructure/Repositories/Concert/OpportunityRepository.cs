using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Data.Application;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Concert;

public class OpportunityRepository : Repository<OpportunityEntity>, IOpportunityRepository
{
    private readonly TimeProvider timeProvider;
    private readonly IReadDbContext readContext;

    public OpportunityRepository(ApplicationDbContext context, TimeProvider timeProvider, IReadDbContext readContext) : base(context)
    {
        this.timeProvider = timeProvider;
        this.readContext = readContext;
    }

    public async Task<IPagination<OpportunityEntity>> GetActiveByVenueIdAsync(int id, IPageParams pageParams)
    {
        var query = context.Opportunities
            .Where(o => o.VenueId == id && o.Period.Start >= timeProvider.GetUtcNow())
            .Where(o => !context.ConcertBookings.Any(b => b.Application.OpportunityId == o.Id))
            .Include(o => o.Contract)
            .Include(o => o.OpportunityGenres)
            .ThenInclude(og => og.Genre)
            .OrderBy(o => o.Period.Start);

        return await query.ToPaginationAsync(pageParams);
    }

    public async Task<UserEntity?> GetOwnerByIdAsync(int opportunityId)
    {
        return await readContext.Users
            .OfType<VenueManagerEntity>()
            .Where(vm => readContext.Venues.Any(v => v.UserId == vm.Id && v.Opportunities.Any(o => o.Id == opportunityId)))
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
            .Where(o => o.Applications.Any(a => a.Id == id))
            .FirstOrDefaultAsync();
    }
}
