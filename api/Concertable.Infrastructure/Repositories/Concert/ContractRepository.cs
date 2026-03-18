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
        return await context.Contracts.FindAsync(opportunityId);
    }
}
