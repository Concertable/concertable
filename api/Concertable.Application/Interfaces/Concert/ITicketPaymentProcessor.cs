using Concertable.Application.Results;

namespace Concertable.Application.Interfaces.Concert;

public interface ITicketPaymentProcessor
{
    Task<PaymentResult> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price);
}
