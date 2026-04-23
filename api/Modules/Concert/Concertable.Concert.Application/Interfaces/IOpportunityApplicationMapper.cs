using Concertable.Concert.Application.DTOs;

namespace Concertable.Concert.Application.Interfaces;

internal interface IOpportunityApplicationMapper
{
    OpportunityApplicationDto ToDto(OpportunityApplicationEntity application);
    IEnumerable<OpportunityApplicationDto> ToDtos(IEnumerable<OpportunityApplicationEntity> applications);
}
