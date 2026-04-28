using Concertable.Payment.Contracts.Events;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class SettlementPaymentService : IPaymentSucceededProcessor
{
    private readonly IConcertWorkflowModule concertWorkflowModule;

    public SettlementPaymentService(IConcertWorkflowModule concertWorkflowModule)
    {
        this.concertWorkflowModule = concertWorkflowModule;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var bookingId = int.Parse(@event.Metadata["bookingId"]);
        await concertWorkflowModule.SettleAsync(bookingId, ct);
    }
}
