using Concertable.Core.Entities;
using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertValidator
{
    Task<Result> CanUpdateAsync(Core.Entities.ConcertEntity concert, int newTotalTickets);
    Task<Result> CanPostAsync(Core.Entities.ConcertEntity concert);
}
