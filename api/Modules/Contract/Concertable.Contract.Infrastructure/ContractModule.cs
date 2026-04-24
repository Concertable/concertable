using Concertable.Contract.Application.Interfaces;

namespace Concertable.Contract.Infrastructure;

internal sealed class ContractModule(IContractRepository contractRepository, IContractMapper mapper) : IContractModule
{
    public async Task<IContract?> GetByOpportunityAsync(int opportunityId, CancellationToken ct = default)
    {
        var entity = await contractRepository.GetByOpportunityIdAsync(opportunityId, ct);
        return entity is null ? null : mapper.ToContract(entity);
    }
}
