using Concertable.Core.Entities;
using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertDraftService
{
    Task<Result<ConcertEntity>> CreateAsync(int bookingId);
}
