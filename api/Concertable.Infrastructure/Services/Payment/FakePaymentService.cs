using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;

namespace Infrastructure.Services.Payment;

public class FakePaymentService : IPaymentService
{
    public Task<PaymentResponse> ProcessAsync(TransactionRequest request) =>
        Task.FromResult(new PaymentResponse
        {
            Success = true,
            RequiresAction = false,
            Message = "Fake payment processed",
            TransactionId = $"fake_{Guid.NewGuid()}"
        });
}
