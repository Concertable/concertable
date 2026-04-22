using Concertable.Core.Entities;
using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertValidator
{
    Task<Result> CanUpdateAsync(ConcertEntity concert, int newTotalTickets);
    Task<Result> CanPostAsync(ConcertEntity concert);
}
