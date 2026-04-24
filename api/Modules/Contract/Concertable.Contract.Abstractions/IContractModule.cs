namespace Concertable.Contract.Abstractions;

public interface IContractModule
{
    Task<IContract?> GetByOpportunityAsync(int opportunityId, CancellationToken ct = default);
}
