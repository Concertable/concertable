using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Application.Interfaces.Concert;

public interface IContractService
{
    Task<IContract> GetByOpportunityIdAsync(int opportunityId);
}
