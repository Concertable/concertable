using Concertable.Payment.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class EscrowPaymentProcessor : IPaymentSucceededProcessor
{
    private readonly IConcertWorkflowModule concertWorkflowModule;
    private readonly ILogger<EscrowPaymentProcessor> logger;

    public EscrowPaymentProcessor(IConcertWorkflowModule concertWorkflowModule, ILogger<EscrowPaymentProcessor> logger)
    {
        this.concertWorkflowModule = concertWorkflowModule;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var bookingId = int.Parse(@event.Metadata["bookingId"]);
        logger.LogDebug(
            "Escrow webhook received: payment intent {TransactionId} for booking {BookingId}",
            @event.TransactionId, bookingId);
        await concertWorkflowModule.SettleAsync(bookingId, ct);
    }
}
