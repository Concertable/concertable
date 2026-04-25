using Concertable.Contract.Application.Interfaces;
using Concertable.Contract.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Contract.Infrastructure.Repositories;

internal class ContractRepository(ContractDbContext context)
    : IdModuleRepository<ContractEntity, ContractDbContext>(context), IContractRepository
{
    public Task<ContractEntity?> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default) =>
        context.Contracts.FirstOrDefaultAsync(c => c.OpportunityId == opportunityId, ct);
}
