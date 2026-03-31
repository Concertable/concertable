using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertOpportunityMapper
{
    ConcertOpportunityDto ToDto(ConcertOpportunityEntity opportunity);
    IPagination<ConcertOpportunityDto> ToDtos(IPagination<ConcertOpportunityEntity> opportunities);
}
