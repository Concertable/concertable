using Concertable.Application.DTOs;
using Concertable.Application.Results;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityMapper
{
    OpportunityDto ToDto(OpportunityEntity opportunity);
    IPagination<OpportunityDto> ToDtos(IPagination<OpportunityEntity> opportunities);
}
