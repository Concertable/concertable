using Concertable.Concert.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Interfaces.Webhook;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services.Webhook;

internal class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly IConcertWorkflowModule concertWorkflowModule;

    public SettlementWebhookHandler(
        ITransactionService transactionService,
        IConcertWorkflowModule concertWorkflowModule)
    {
        this.transactionService = transactionService;
        this.concertWorkflowModule = concertWorkflowModule;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        await transactionService.CompleteAsync(intent.Id);
        await concertWorkflowModule.SettleAsync(int.Parse(intent.Metadata["bookingId"]), cancellationToken);
    }
}
