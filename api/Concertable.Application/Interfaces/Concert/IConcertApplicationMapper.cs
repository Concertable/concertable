using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertApplicationMapper
{
    IConcertApplication ToDto(ConcertApplicationEntity application);
    IConcertApplication ToArtistDto(ConcertApplicationEntity application);
    IEnumerable<IConcertApplication> ToDtos(IEnumerable<ConcertApplicationEntity> applications);
    IEnumerable<IConcertApplication> ToArtistDtos(IEnumerable<ConcertApplicationEntity> applications);
}
