using Concertable.Concert.Api.Responses;
using Concertable.Concert.Application.DTOs;

namespace Concertable.Concert.Api.Mappers;

internal static class ApplicationResponseMappers
{
    public static ApplicationResponse ToResponse(this ApplicationDto dto) =>
        new(dto.Id,
            dto.Artist,
            new OpportunitySummaryResponse(
                dto.Opportunity.Id,
                dto.Opportunity.StartDate,
                dto.Opportunity.EndDate,
                dto.Opportunity.Contract),
            dto.Status);

    public static IEnumerable<ApplicationResponse> ToResponses(this IEnumerable<ApplicationDto> dtos) =>
        dtos.Select(d => d.ToResponse());
}
