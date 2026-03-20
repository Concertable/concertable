using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Concert;

public class ContractRepository : Repository<ContractEntity>, IContractRepository
{
    public ContractRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ContractEntity?> GetByOpportunityIdAsync(int opportunityId)
    {
        return await context.ConcertOpportunities
            .Where(o => o.Id == opportunityId)
            .Select(o => o.Contract)
            .FirstOrDefaultAsync();
    }

    public async Task<ContractEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => c.Application.Opportunity.Contract)
            .FirstOrDefaultAsync();
    }
}
