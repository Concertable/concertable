using Concertable.Contract.Application.Interfaces;
using Concertable.Shared.Exceptions;

namespace Concertable.Contract.Application.Services;

internal sealed class ContractService(IContractModule contractModule) : IContractService
{
    public async Task<IContract> GetByOpportunityIdAsync(int opportunityId)
        => await contractModule.GetByOpportunityAsync(opportunityId)
            ?? throw new NotFoundException($"No contract for opportunity {opportunityId}");
}
