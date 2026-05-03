using FluentResults;

namespace Concertable.Concert.Infrastructure.Validators;

internal class ConcertValidator : IConcertValidator
{
    public Result CanUpdate(ConcertEntity concert, int newTotalTickets)
    {
        int ticketsSold = concert.TotalTickets - concert.AvailableTickets;

        return newTotalTickets < ticketsSold
            ? Result.Fail("Cannot reduce TotalTickets below the number of tickets already sold")
            : Result.Ok();
    }

    public Result CanPost(ConcertEntity concert)
    {
        var errors = new List<string>();

        if (concert.Booking.Status != BookingStatus.Confirmed)
            errors.Add("Concert cannot be posted until the booking is confirmed");

        if (concert.DatePosted is not null)
            errors.Add("Concert has already been posted");

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }
}
