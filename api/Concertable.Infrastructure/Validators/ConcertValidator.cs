using FluentResults;

namespace Concertable.Infrastructure.Validators;

internal class ConcertValidator : IConcertValidator
{
    public Task<Result> CanUpdateAsync(ConcertEntity concert, int newTotalTickets)
    {
        int ticketsSold = concert.TotalTickets - concert.AvailableTickets;

        return newTotalTickets < ticketsSold
            ? Task.FromResult(Result.Fail("Cannot reduce TotalTickets below the number of tickets already sold"))
            : Task.FromResult(Result.Ok());
    }

    public Task<Result> CanPostAsync(ConcertEntity concert)
    {
        var errors = new List<string>();

        if (concert.Booking.Status != BookingStatus.Confirmed)
            errors.Add("Concert cannot be posted until the booking is confirmed");

        if (concert.DatePosted is not null)
            errors.Add("Concert has already been posted");

        return Task.FromResult(errors.Count > 0 ? Result.Fail(errors) : Result.Ok());
    }
}
