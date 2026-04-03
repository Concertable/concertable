using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityApplicationMapper
{
    OpportunityApplicationDto ToDto(OpportunityApplicationEntity application);
    IEnumerable<OpportunityApplicationDto> ToDtos(IEnumerable<OpportunityApplicationEntity> applications);
}
