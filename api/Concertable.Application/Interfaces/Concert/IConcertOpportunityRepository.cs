using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertOpportunityRepository : IRepository<ConcertOpportunityEntity>
{
    new Task<ConcertOpportunityEntity?> GetByIdAsync(int id);
    Task<IEnumerable<ConcertOpportunityEntity>> GetActiveByVenueIdAsync(int id);
    Task<ConcertOpportunityEntity?> GetWithVenueByIdAsync(int id);
    Task<ConcertOpportunityEntity?> GetByApplicationIdAsync(int id);
    Task<UserEntity?> GetOwnerByIdAsync(int id);
}
