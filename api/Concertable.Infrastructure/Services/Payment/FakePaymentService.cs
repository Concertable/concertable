using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Interfaces;
using FluentResults;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

public class FakePaymentService : IPaymentService
{
    private readonly IWebhookQueue webhookQueue;

    public FakePaymentService(IWebhookQueue webhookQueue)
    {
        this.webhookQueue = webhookQueue;
    }

    public async Task<Result<PaymentResponse>> ProcessAsync(TransactionRequest request)
    {
        var transactionId = $"pi_fake_{Guid.NewGuid():N}";

        await webhookQueue.EnqueueAsync(new Event
        {
            Id = $"evt_fake_{Guid.NewGuid():N}",
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = new PaymentIntent
                {
                    Id = transactionId,
                    Status = "succeeded",
                    AmountReceived = (long)(request.Amount * 100),
                    Metadata = request.Metadata
                }
            }
        });

        return Result.Ok(new PaymentResponse
        {
            RequiresAction = false,
            TransactionId = transactionId
        });
    }
}
