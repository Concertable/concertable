using Application.DTOs;
using Application.Interfaces.Concert;

namespace Application.Interfaces.Concert;

public interface IContractService
{
    Task<IContract> GetByOpportunityIdAsync(int opportunityId);
    Task CreateAsync(IContract contract);
}
