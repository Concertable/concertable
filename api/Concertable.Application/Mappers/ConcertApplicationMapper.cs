using Application.DTOs;
using Application.Interfaces.Concert;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public class ConcertApplicationMapper : IConcertApplicationMapper
{
    private readonly IConcertOpportunityMapper opportunityMapper;

    public ConcertApplicationMapper(IConcertOpportunityMapper opportunityMapper)
    {
        this.opportunityMapper = opportunityMapper;
    }

    public ConcertApplicationDto ToDto(ConcertApplicationEntity application) => new(
        application.Id,
        application.Artist.ToDto(),
        opportunityMapper.ToDto(application.Opportunity),
        application.Concert != null ? ApplicationStatus.Accepted : ApplicationStatus.Pending
    );

    public ArtistConcertApplicationDto ToArtistDto(ConcertApplicationEntity application) => new(
        application.Id,
        application.Artist.ToDto(),
        opportunityMapper.ToWithVenueDto(application.Opportunity),
        application.Concert != null ? ApplicationStatus.Accepted : ApplicationStatus.Pending
    );

    public IEnumerable<ConcertApplicationDto> ToDtos(IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(ToDto);

    public IEnumerable<ArtistConcertApplicationDto> ToArtistDtos(IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(ToArtistDto);
}
