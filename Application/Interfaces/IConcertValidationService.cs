using Application.Responses;
using Core.Entities;

namespace Application.Interfaces;

public interface IConcertValidationService
{
    Task<ValidationResponse> CanUpdateAsync(Concert concert, int newTotalTickets);
    Task<ValidationResponse> CanPostAsync(Concert concert);
}
