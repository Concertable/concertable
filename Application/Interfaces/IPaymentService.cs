using Application.Requests;
using Application.Responses;

namespace Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(TransactionRequest request);
}
