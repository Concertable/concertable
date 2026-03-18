using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertOpportunityMapper
{
    ConcertOpportunityDto ToDto(ConcertOpportunityEntity opportunity);
    OpportunityWithVenueDto ToWithVenueDto(ConcertOpportunityEntity opportunity);
    IEnumerable<ConcertOpportunityDto> ToDtos(IEnumerable<ConcertOpportunityEntity> opportunities);
    ConcertOpportunityEntity ToEntity(ConcertOpportunityDto dto);
}
