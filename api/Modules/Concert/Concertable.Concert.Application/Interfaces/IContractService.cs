namespace Concertable.Concert.Application.Interfaces;

internal interface IContractService
{
    Task<IContract> GetByOpportunityIdAsync(int opportunityId);
}
