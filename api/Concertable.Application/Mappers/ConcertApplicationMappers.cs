using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public static class ConcertApplicationMappers
{
    public static ConcertApplicationDto ToDto(this ConcertApplicationEntity application) => new(
        application.Id,
        application.Artist.ToDto(),
        application.Opportunity.ToDto(),
        application.Concert != null ? ApplicationStatus.Accepted : ApplicationStatus.Pending
    );

    public static ArtistConcertApplicationDto ToArtistConcertApplicationDto(this ConcertApplicationEntity application) => new(
        application.Id,
        application.Artist.ToDto(),
        application.Opportunity.ToWithVenueDto(),
        application.Concert != null ? ApplicationStatus.Accepted : ApplicationStatus.Pending
    );

    public static IEnumerable<ConcertApplicationDto> ToDtos(this IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(a => a.ToDto());

    public static IEnumerable<ArtistConcertApplicationDto> ToArtistConcertApplicationDtos(this IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(a => a.ToArtistConcertApplicationDto());
}
