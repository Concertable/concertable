using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IConcertOpportunityService
{
    Task CreateAsync(ConcertOpportunityDto opportunityDto);
    Task CreateMultipleAsync(IEnumerable<ConcertOpportunityDto> opportunitiesDto);
    Task<IEnumerable<ConcertOpportunityDto>> GetActiveByVenueIdAsync(int id);
    Task<User> GetOwnerByIdAsync(int id);
    Task<ConcertOpportunity> GetByIdAsync(int id);
}
