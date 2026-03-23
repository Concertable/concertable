using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Infrastructure.Interfaces;
using Stripe;

namespace Infrastructure.Services.Webhook;

public class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ISettlementProcessor settlementProcessor;
    private readonly TimeProvider timeProvider;

    public SettlementWebhookHandler(
        ITransactionService transactionService,
        ISettlementProcessor settlementProcessor,
        TimeProvider timeProvider)
    {
        this.transactionService = transactionService;
        this.settlementProcessor = settlementProcessor;
        this.timeProvider = timeProvider;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        await transactionService.CompleteAsync(intent.Id);
        await settlementProcessor.SettleAsync(int.Parse(intent.Metadata["applicationId"]));
    }
}
