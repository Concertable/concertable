using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Infrastructure.Interfaces;
using Stripe;

namespace Concertable.Infrastructure.Services.Webhook;

public class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ISettlementProcessor settlementProcessor;

    public SettlementWebhookHandler(
        ITransactionService transactionService,
        ISettlementProcessor settlementProcessor)
    {
        this.transactionService = transactionService;
        this.settlementProcessor = settlementProcessor;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        await transactionService.CompleteAsync(intent.Id);
        await settlementProcessor.SettleAsync(int.Parse(intent.Metadata["applicationId"]));
    }
}
