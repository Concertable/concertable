using Application.Responses;

namespace Application.Interfaces.Concert;

public interface ITicketPaymentStrategy : IContractStrategy
{
    Task<PaymentResponse> PayAsync(int concertId, int quantity, string paymentMethodId, decimal price);
}
