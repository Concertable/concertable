using Application.Requests;
using Application.Responses;

namespace Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(TransactionRequest request);
}
