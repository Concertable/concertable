using Concertable.Application.Interfaces;
using FluentResults;

namespace Concertable.Infrastructure.Validators;

internal class TicketValidator : ITicketValidator
{
    private readonly IConcertRepository concertRepository;
    private readonly TimeProvider timeProvider;

    public TicketValidator(IConcertRepository concertRepository, TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<Result> CanPurchaseTicketAsync(int concertId, int? quantity = null)
    {
        var concert = await concertRepository.GetDtoByIdAsync(concertId);

        if (concert is null)
            return Result.Fail("Concert does not exist.");

        var errors = new List<string>();

        if (concert.DatePosted is null)
            errors.Add("Concert is not posted yet");

        if (concert.StartDate < timeProvider.GetUtcNow())
            errors.Add("You cannot purchase a Ticket for a Concert that's already passed");

        if (concert.AvailableTickets <= 0)
            errors.Add("No Tickets Available for Concert");

        if (quantity.HasValue && concert.AvailableTickets - quantity.Value < 0)
            errors.Add($"Not enough tickets available. Only {concert.AvailableTickets} tickets are available");

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }
}
