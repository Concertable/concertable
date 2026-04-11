using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Results;

namespace Concertable.Infrastructure.Validators;

public class TicketValidator : ITicketValidator
{
    private readonly IConcertRepository concertRepository;
    private readonly TimeProvider timeProvider;

    public TicketValidator(IConcertRepository concertRepository, TimeProvider timeProvider)
    {
        this.concertRepository = concertRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<ValidationResult> CanPurchaseTicketAsync(int concertId, int? quantity = null)
    {
        var result = new ValidationResult();
        var concert = await concertRepository.GetDetailsByIdAsync(concertId);

        if (concert is null)
        {
            result.AddError("Concert does not exist.");
            return result;
        }

        if (concert.DatePosted is null)
            result.AddError("Concert is not posted yet");

        if (concert.StartDate < timeProvider.GetUtcNow())
            result.AddError("You cannot purchase a Ticket for a Concert that's already passed");

        if (concert.AvailableTickets <= 0)
            result.AddError("No Tickets Available for Concert");

        if (quantity.HasValue && concert.AvailableTickets - quantity.Value < 0)
            result.AddError($"Not enough tickets available. Only {concert.AvailableTickets} tickets are available");

        return result;
    }
}
