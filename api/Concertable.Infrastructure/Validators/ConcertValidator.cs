using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Infrastructure.Validators;

public class ConcertValidator : IConcertValidator
{
    public Task<ValidationResult> CanUpdateAsync(ConcertEntity concert, int newTotalTickets)
    {
        var result = new ValidationResult();
        int ticketsSold = concert.TotalTickets - concert.AvailableTickets;

        if (newTotalTickets < ticketsSold)
            result.AddError("Cannot reduce TotalTickets below the number of tickets already sold");

        return Task.FromResult(result);
    }

    public Task<ValidationResult> CanPostAsync(ConcertEntity concert)
    {
        var result = new ValidationResult();

        if (concert.Application.Status != ApplicationStatus.Settled)
            result.AddError("Concert cannot be posted until the application is settled");

        if (concert.DatePosted is not null)
            result.AddError("Concert has already been posted");

        return Task.FromResult(result);
    }
}
