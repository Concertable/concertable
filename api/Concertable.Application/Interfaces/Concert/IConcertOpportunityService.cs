using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertOpportunityService
{
    Task CreateAsync(ConcertOpportunityDto opportunityDto);
    Task CreateMultipleAsync(IEnumerable<ConcertOpportunityDto> opportunitiesDto);
    Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id);
    Task<UserEntity> GetOwnerByIdAsync(int id);
    Task<ConcertOpportunityEntity> GetByIdAsync(int id);
}
