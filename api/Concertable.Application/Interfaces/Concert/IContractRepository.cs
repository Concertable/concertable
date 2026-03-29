using Concertable.Application.Interfaces;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Concert;

public interface IContractRepository : IRepository<ContractEntity>
{
    Task<T?> GetByOpportunityIdAsync<T>(int opportunityId) where T : ContractEntity;
    Task<T?> GetByConcertIdAsync<T>(int concertId) where T : ContractEntity;
    Task<T?> GetByApplicationIdAsync<T>(int applicationId) where T : ContractEntity;
    Task<ContractType?> GetTypeByConcertIdAsync(int concertId);
    Task<ContractType?> GetTypeByApplicationIdAsync(int applicationId);
}
