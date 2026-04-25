using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Interfaces.Webhook;
using Stripe;

namespace Concertable.Concert.Infrastructure.Services.Webhook;

internal class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ISettlementDispatcher settlementDispatcher;

    public SettlementWebhookHandler(
        ITransactionService transactionService,
        ISettlementDispatcher settlementDispatcher)
    {
        this.transactionService = transactionService;
        this.settlementDispatcher = settlementDispatcher;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        await transactionService.CompleteAsync(intent.Id);
        await settlementDispatcher.SettleAsync(int.Parse(intent.Metadata["bookingId"]));
    }
}
