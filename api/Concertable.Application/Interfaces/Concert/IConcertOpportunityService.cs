using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertOpportunityService
{
    Task CreateAsync(ConcertOpportunityRequest request);
    Task CreateMultipleAsync(IEnumerable<ConcertOpportunityRequest> requests);
    Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id);
    Task<UserEntity> GetOwnerByIdAsync(int id);
    Task<ConcertOpportunityEntity> GetByIdAsync(int id);
}
