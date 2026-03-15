using Application.Responses;
using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertValidator
{
    Task<ValidationResult> CanUpdateAsync(Core.Entities.ConcertEntity concert, int newTotalTickets);
    Task<ValidationResult> CanPostAsync(Core.Entities.ConcertEntity concert);
}
