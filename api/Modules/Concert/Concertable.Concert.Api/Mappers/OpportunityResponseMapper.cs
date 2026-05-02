using Concertable.Concert.Api.Responses;
using Concertable.Concert.Application.DTOs;

namespace Concertable.Concert.Api.Mappers;

internal sealed class OpportunityResponseMapper : IOpportunityResponseMapper
{
    public OpportunityResponse ToResponse(OpportunityDto dto)
    {
        var actions = new OpportunityActions(
            Apply: new ActionLink($"/api/Opportunity/{dto.Id}/applications", "POST"));

        return new OpportunityResponse(
            dto.Id,
            dto.VenueId,
            dto.ContractId,
            dto.Contract,
            dto.StartDate,
            dto.EndDate,
            dto.Genres,
            actions);
    }

    public IEnumerable<OpportunityResponse> ToResponses(IEnumerable<OpportunityDto> dtos) =>
        dtos.Select(ToResponse);
}
