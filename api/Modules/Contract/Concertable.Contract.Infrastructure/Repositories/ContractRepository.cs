using Concertable.Concert.Infrastructure.Data;
using Concertable.Contract.Application.Interfaces;
using Concertable.Contract.Domain;
using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Contract.Infrastructure.Repositories;

internal class ContractRepository : IdModuleRepository<ContractEntity, ConcertDbContext>, IContractRepository
{
    public ContractRepository(ConcertDbContext context) : base(context) { }

    public Task<ContractEntity?> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default) =>
        context.Contracts.FirstOrDefaultAsync(c => c.OpportunityId == opportunityId, ct);
}
