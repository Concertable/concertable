using Application.Responses;

namespace Application.Interfaces.Concert;

public interface ITicketPaymentProcessor
{
    Task<PaymentResponse> PayAsync(int concertId, int quantity, string paymentMethodId, decimal price);
}
