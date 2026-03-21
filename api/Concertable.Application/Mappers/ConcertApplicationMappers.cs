using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class ConcertApplicationMappers
{
    public static ConcertApplicationDto ToDto(this ConcertApplicationEntity application) =>
        new(application.Id, application.Artist.ToDto(), application.Opportunity.ToDto(), application.Opportunity.Contract.ContractType, application.Status);

    public static IEnumerable<ConcertApplicationDto> ToDtos(this IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(a => a.ToDto());
}
