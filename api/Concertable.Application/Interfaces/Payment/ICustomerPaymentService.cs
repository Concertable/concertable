using Concertable.Application.Results;

namespace Concertable.Application.Interfaces.Payment;

public interface ICustomerPaymentService
{
    Task<PaymentResult> PayVenueManagerByConcertIdAsync(int concertId, int quantity, string paymentMethodId, decimal price);
}
