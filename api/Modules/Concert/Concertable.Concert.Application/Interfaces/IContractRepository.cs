namespace Concertable.Concert.Application.Interfaces;

internal interface IContractRepository : IIdRepository<ContractEntity>
{
    Task<T?> GetByOpportunityIdAsync<T>(int opportunityId) where T : ContractEntity;
    Task<T?> GetByConcertIdAsync<T>(int concertId) where T : ContractEntity;
    Task<T?> GetByApplicationIdAsync<T>(int applicationId) where T : ContractEntity;
    Task<ContractType?> GetTypeByConcertIdAsync(int concertId);
    Task<ContractType?> GetTypeByApplicationIdAsync(int applicationId);
    Task<ContractType?> GetTypeByBookingIdAsync(int bookingId);
}
