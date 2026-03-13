using Application.Responses;
using Core.Entities;

namespace Application.Interfaces;

public interface IConcertValidator
{
    Task<ValidationResult> CanUpdateAsync(Concert concert, int newTotalTickets);
    Task<ValidationResult> CanPostAsync(Concert concert);
}
