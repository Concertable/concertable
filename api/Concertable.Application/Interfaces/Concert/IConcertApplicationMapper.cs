using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertApplicationMapper
{
    ConcertApplicationDto ToDto(ConcertApplicationEntity application);
    IEnumerable<ConcertApplicationDto> ToDtos(IEnumerable<ConcertApplicationEntity> applications);
}
