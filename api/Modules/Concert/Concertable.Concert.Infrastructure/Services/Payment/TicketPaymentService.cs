using Concertable.Payment.Contracts.Events;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class TicketPaymentService : IPaymentSucceededStrategy
{
    private readonly ITicketService ticketService;
    private readonly ITicketNotifier notifier;

    public TicketPaymentService(ITicketService ticketService, ITicketNotifier notifier)
    {
        this.ticketService = ticketService;
        this.notifier = notifier;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var meta = @event.Metadata;

        var result = await ticketService.CompleteAsync(new()
        {
            EntityId = int.Parse(meta["concertId"]),
            FromUserId = Guid.Parse(meta["fromUserId"]),
            FromEmail = meta.GetValueOrDefault("fromUserEmail", string.Empty),
            Quantity = meta.TryGetValue("quantity", out var q) ? int.Parse(q) : null
        });

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        await notifier.TicketPurchasedAsync(meta["fromUserId"], result.Value);
    }
}
