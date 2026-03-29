using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertOpportunityService
{
    Task<ConcertOpportunityDto> CreateAsync(ConcertOpportunityRequest request);
    Task CreateMultipleAsync(IEnumerable<ConcertOpportunityRequest> requests);
    Task<ConcertOpportunityDto> UpdateAsync(int id, ConcertOpportunityRequest request);
    Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id);
    Task<ConcertOpportunityDto> GetByIdAsync(int id);
    Task<UserEntity> GetOwnerByIdAsync(int id);
}
