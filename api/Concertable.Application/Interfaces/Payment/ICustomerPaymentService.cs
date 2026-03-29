using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Payment;

public interface ICustomerPaymentService
{
    Task<PaymentResponse> PayVenueManagerByConcertIdAsync(int concertId, int quantity, string paymentMethodId, decimal price);
}
