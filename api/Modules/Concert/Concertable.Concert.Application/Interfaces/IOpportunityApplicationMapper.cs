using Concertable.Concert.Application.DTOs;

namespace Concertable.Concert.Application.Interfaces;

internal interface IOpportunityApplicationMapper
{
    Task<OpportunityApplicationDto> ToDtoAsync(OpportunityApplicationEntity application);
    Task<IEnumerable<OpportunityApplicationDto>> ToDtosAsync(IEnumerable<OpportunityApplicationEntity> applications);
}
