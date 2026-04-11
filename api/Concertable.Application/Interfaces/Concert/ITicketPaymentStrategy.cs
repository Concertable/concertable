using Concertable.Application.Results;

namespace Concertable.Application.Interfaces.Concert;

public interface ITicketPaymentStrategy : IContractStrategy
{
    Task<PaymentResult> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price);
}
