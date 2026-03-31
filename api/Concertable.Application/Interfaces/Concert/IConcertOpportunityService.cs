using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertOpportunityService
{
    Task<ConcertOpportunityDto> CreateAsync(ConcertOpportunityRequest request);
    Task CreateMultipleAsync(IEnumerable<ConcertOpportunityRequest> requests);
    Task<ConcertOpportunityDto> UpdateAsync(int id, ConcertOpportunityRequest request);
    Task<IPagination<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id, IPageParams pageParams);
    Task<ConcertOpportunityDto> GetByIdAsync(int id);
    Task<UserEntity> GetOwnerByIdAsync(int id);
}
