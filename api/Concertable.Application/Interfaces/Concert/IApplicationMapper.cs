using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IApplicationMapper : IContractStrategy
{
    IConcertApplication ToDto(ConcertApplicationEntity application);
    IConcertApplication ToArtistDto(ConcertApplicationEntity application);
}
