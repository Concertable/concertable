using Concertable.Application.Results;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertValidator
{
    Task<ValidationResult> CanUpdateAsync(Core.Entities.ConcertEntity concert, int newTotalTickets);
    Task<ValidationResult> CanPostAsync(Core.Entities.ConcertEntity concert);
}
