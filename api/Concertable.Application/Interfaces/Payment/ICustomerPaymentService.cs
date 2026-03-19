using Application.Responses;

namespace Application.Interfaces.Payment;

public interface ICustomerPaymentService
{
    Task<PaymentResponse> PayVenueManagerByConcertIdAsync(int concertId, int quantity, string paymentMethodId, decimal price);
}
