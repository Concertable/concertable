using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityRepository : IRepository<OpportunityEntity>
{
    new Task<OpportunityEntity?> GetByIdAsync(int id);
    Task<IPagination<OpportunityEntity>> GetActiveByVenueIdAsync(int id, IPageParams pageParams);
    Task<OpportunityEntity?> GetWithVenueByIdAsync(int id);
    Task<OpportunityEntity?> GetByApplicationIdAsync(int id);
    Task<UserEntity?> GetOwnerByIdAsync(int id);
}
