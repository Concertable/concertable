using Concertable.Payment.Contracts.Events;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class TicketPaymentService : IPaymentSucceededStrategy
{
    private readonly ITicketService ticketService;
    private readonly ITicketNotificationService notificationService;

    public TicketPaymentService(ITicketService ticketService, ITicketNotificationService notificationService)
    {
        this.ticketService = ticketService;
        this.notificationService = notificationService;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var meta = @event.Metadata;

        var result = await ticketService.CompleteAsync(new()
        {
            EntityId = int.Parse(meta["concertId"]),
            FromUserId = Guid.Parse(meta["fromUserId"]),
            FromEmail = meta.GetValueOrDefault("fromUserEmail", string.Empty),
            Quantity = int.Parse(meta["quantity"])
        });

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        await notificationService.TicketPurchasedAsync(meta["fromUserId"], result.Value);
    }
}
