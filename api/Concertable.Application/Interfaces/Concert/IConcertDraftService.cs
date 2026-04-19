using Concertable.Core.Entities;
using Concertable.Core.Enums;
using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertDraftService
{
    Task<Result<ConcertEntity>> CreateAsync(int applicationId);
}
