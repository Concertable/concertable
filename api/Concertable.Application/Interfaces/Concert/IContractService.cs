using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Application.Interfaces.Concert;

public interface IContractService
{
    Task<IContract> GetByOpportunityIdAsync(int opportunityId);
    Task AddAsync(IContract contract, int opportunityId);
    Task CreateAsync(IContract contract, int opportunityId);
    Task UpdateAsync(IContract contract, int opportunityId);
}
