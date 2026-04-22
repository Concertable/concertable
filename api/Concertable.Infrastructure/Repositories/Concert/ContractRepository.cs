using Concertable.Infrastructure.Data;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories.Concert;

internal class ContractRepository : Repository<ContractEntity>, IContractRepository
{
    public ContractRepository(ApplicationDbContext context) : base(context) { }

    public async Task<T?> GetByOpportunityIdAsync<T>(int opportunityId) where T : ContractEntity
    {
        return await context.Opportunities
            .Where(o => o.Id == opportunityId)
            .Select(o => o.Contract)
            .OfType<T>()
            .FirstOrDefaultAsync();
    }

    public async Task<T?> GetByConcertIdAsync<T>(int concertId) where T : ContractEntity
    {
        return await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => c.Booking.Application.Opportunity.Contract)
            .OfType<T>()
            .FirstOrDefaultAsync();
    }

    public async Task<T?> GetByApplicationIdAsync<T>(int applicationId) where T : ContractEntity
    {
        return await context.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => a.Opportunity.Contract)
            .OfType<T>()
            .FirstOrDefaultAsync();
    }

    public async Task<ContractType?> GetTypeByConcertIdAsync(int concertId)
    {
        var opportunityId = await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (int?)c.Booking.Application.OpportunityId)
            .FirstOrDefaultAsync();

        if (opportunityId is null) return null;

        var contract = await context.Contracts
            .Where(c => c.Id == opportunityId)
            .FirstOrDefaultAsync();

        return contract?.ContractType;
    }

    public async Task<ContractType?> GetTypeByApplicationIdAsync(int applicationId)
    {
        var opportunityId = await context.OpportunityApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (int?)a.OpportunityId)
            .FirstOrDefaultAsync();

        if (opportunityId is null) return null;

        var contract = await context.Contracts
            .Where(c => c.Id == opportunityId)
            .FirstOrDefaultAsync();

        return contract?.ContractType;
    }

    public async Task<ContractType?> GetTypeByBookingIdAsync(int bookingId)
    {
        var opportunityId = await context.ConcertBookings
            .Where(b => b.Id == bookingId)
            .Select(b => (int?)b.Application.OpportunityId)
            .FirstOrDefaultAsync();

        if (opportunityId is null) return null;

        var contract = await context.Contracts
            .Where(c => c.Id == opportunityId)
            .FirstOrDefaultAsync();

        return contract?.ContractType;
    }
}
