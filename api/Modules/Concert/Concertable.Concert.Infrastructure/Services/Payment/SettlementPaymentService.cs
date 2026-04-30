using Concertable.Payment.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class SettlementPaymentService : IPaymentSucceededProcessor
{
    private readonly IConcertWorkflowModule concertWorkflowModule;
    private readonly ILogger<SettlementPaymentService> logger;

    public SettlementPaymentService(IConcertWorkflowModule concertWorkflowModule, ILogger<SettlementPaymentService> logger)
    {
        this.concertWorkflowModule = concertWorkflowModule;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var bookingId = int.Parse(@event.Metadata["bookingId"]);
        logger.LogDebug(
            "Settlement webhook received: payment intent {TransactionId} for booking {BookingId}",
            @event.TransactionId, bookingId);
        await concertWorkflowModule.SettleAsync(bookingId, ct);
    }
}
