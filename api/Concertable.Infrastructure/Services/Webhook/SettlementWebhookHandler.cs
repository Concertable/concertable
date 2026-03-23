using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Infrastructure.Interfaces;
using Stripe;

namespace Infrastructure.Services.Webhook;

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
