namespace Concertable.Contract.Application.Interfaces;

internal interface IContractService
{
    Task<IContract> GetByOpportunityIdAsync(int opportunityId);
}
