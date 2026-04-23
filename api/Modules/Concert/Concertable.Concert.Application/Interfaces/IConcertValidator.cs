using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertValidator
{
    Task<Result> CanUpdateAsync(ConcertEntity concert, int newTotalTickets);
    Task<Result> CanPostAsync(ConcertEntity concert);
}
