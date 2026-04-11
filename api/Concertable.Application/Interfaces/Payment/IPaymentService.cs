using Concertable.Application.Requests;
using Concertable.Application.Results;

namespace Concertable.Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(TransactionRequest request);
}
