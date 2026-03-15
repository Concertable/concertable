using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Responses;
using Core.Entities;

namespace Infrastructure.Validators;

public class ConcertValidator : IConcertValidator
{
    public Task<ValidationResult> CanUpdateAsync(ConcertEntity concert, int newTotalTickets)
    {
        var result = new ValidationResult();
        int ticketsSold = concert.TotalTickets - concert.AvailableTickets;

        if (newTotalTickets < ticketsSold)
            result.AddError("TotalTickets", "Cannot reduce TotalTickets below the number of tickets already sold");

        return Task.FromResult(result);
    }

    public Task<ValidationResult> CanPostAsync(ConcertEntity concert)
    {
        var result = new ValidationResult();

        if (concert.DatePosted is not null)
            result.AddError("Concert has already been posted");

        return Task.FromResult(result);
    }
}
