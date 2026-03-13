using Application.Interfaces;
using Application.Responses;
using Core.Entities;

namespace Infrastructure.Services;

public class ConcertValidationService : IConcertValidationService
{
    public Task<ValidationResponse> CanUpdateAsync(Concert concert, int newTotalTickets)
    {
        int ticketsSold = concert.TotalTickets - concert.AvailableTickets;

        if (newTotalTickets < ticketsSold)
            return Task.FromResult(ValidationResponse.Failure("Cannot reduce TotalTickets below the number of tickets already sold"));

        return Task.FromResult(ValidationResponse.Success());
    }

    public Task<ValidationResponse> CanPostAsync(Concert concert)
    {
        if (concert.DatePosted is not null)
            return Task.FromResult(ValidationResponse.Failure("Concert has already been posted"));

        return Task.FromResult(ValidationResponse.Success());
    }
}
