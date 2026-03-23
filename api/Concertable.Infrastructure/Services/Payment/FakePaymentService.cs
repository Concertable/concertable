using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Infrastructure.Interfaces;
using Stripe;

namespace Infrastructure.Services.Payment;

public class FakePaymentService : IPaymentService
{
    private readonly IWebhookQueue webhookQueue;

    public FakePaymentService(IWebhookQueue webhookQueue)
    {
        this.webhookQueue = webhookQueue;
    }

    public async Task<PaymentResponse> ProcessAsync(TransactionRequest request)
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

        return new PaymentResponse
        {
            Success = true,
            RequiresAction = false,
            Message = "Fake payment processed",
            TransactionId = transactionId
        };
    }
}
