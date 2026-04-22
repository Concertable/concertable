using Concertable.Concert.Application.DTOs;

namespace Concertable.Concert.Application.Interfaces;

internal interface IOpportunityMapper
{
    OpportunityDto ToDto(OpportunityEntity opportunity);
    IPagination<OpportunityDto> ToDtos(IPagination<OpportunityEntity> opportunities);
}
