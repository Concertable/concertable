using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Concert;

public interface ITicketPaymentStrategy : IContractStrategy
{
    Task<PaymentResponse> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price);
}
