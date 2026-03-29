using Concertable.Application.Requests;
using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(TransactionRequest request);
}
