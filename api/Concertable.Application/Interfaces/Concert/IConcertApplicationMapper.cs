using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertApplicationMapper
{
    ConcertApplicationDto ToDto(ConcertApplicationEntity application);
    ArtistConcertApplicationDto ToArtistDto(ConcertApplicationEntity application);
    IEnumerable<ConcertApplicationDto> ToDtos(IEnumerable<ConcertApplicationEntity> applications);
    IEnumerable<ArtistConcertApplicationDto> ToArtistDtos(IEnumerable<ConcertApplicationEntity> applications);
}
