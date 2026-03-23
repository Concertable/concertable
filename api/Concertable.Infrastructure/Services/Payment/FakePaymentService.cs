using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Infrastructure.Interfaces;
using System.Text.Json;

namespace Infrastructure.Services.Payment;

public class FakePaymentService : IPaymentService
{
    private readonly IWebhookService webhookService;

    public FakePaymentService(IWebhookService webhookService)
    {
        this.webhookService = webhookService;
    }

    public async Task<PaymentResponse> ProcessAsync(TransactionRequest request)
    {
        var transactionId = $"pi_fake_{Guid.NewGuid():N}";

        var fakeEventJson = JsonSerializer.Serialize(new
        {
            id = $"evt_fake_{Guid.NewGuid():N}",
            type = "payment_intent.succeeded",
            data = new
            {
                @object = new
                {
                    @object = "payment_intent",
                    id = transactionId,
                    status = "succeeded",
                    amount_received = (long)(request.Amount * 100),
                    metadata = request.Metadata
                }
            }
        });

        await webhookService.HandleAsync(fakeEventJson, string.Empty);

        return new PaymentResponse
        {
            Success = true,
            RequiresAction = false,
            Message = "Fake payment processed",
            TransactionId = transactionId
        };
    }
}
