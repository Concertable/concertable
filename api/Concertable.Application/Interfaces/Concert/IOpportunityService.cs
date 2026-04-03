using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityService
{
    Task<OpportunityDto> CreateAsync(OpportunityRequest request);
    Task CreateMultipleAsync(IEnumerable<OpportunityRequest> requests);
    Task<OpportunityDto> UpdateAsync(int id, OpportunityRequest request);
    Task<IPagination<OpportunityDto>> GetActiveByVenueIdAsync(int id, IPageParams pageParams);
    Task<OpportunityDto> GetByIdAsync(int id);
    Task<UserEntity> GetOwnerByIdAsync(int id);
}
