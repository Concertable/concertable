using Application.Interfaces;
using Concertable.Core.Entities.Contracts;

namespace Application.Interfaces.Concert;

public interface IContractRepository : IRepository<ContractEntity>
{
    Task<ContractEntity?> GetByOpportunityIdAsync(int opportunityId);
    Task<ContractEntity?> GetByConcertIdAsync(int concertId);
}
