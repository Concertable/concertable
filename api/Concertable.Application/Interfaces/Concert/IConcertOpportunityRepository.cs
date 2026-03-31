using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertOpportunityRepository : IRepository<ConcertOpportunityEntity>
{
    new Task<ConcertOpportunityEntity?> GetByIdAsync(int id);
    Task<IPagination<ConcertOpportunityEntity>> GetActiveByVenueIdAsync(int id, IPageParams pageParams);
    Task<ConcertOpportunityEntity?> GetWithVenueByIdAsync(int id);
    Task<ConcertOpportunityEntity?> GetByApplicationIdAsync(int id);
    Task<UserEntity?> GetOwnerByIdAsync(int id);
}
