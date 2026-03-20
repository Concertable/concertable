using Application.Responses;

namespace Application.Interfaces.Concert;

public interface IBookingPaymentProcessor
{
    Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId);
}
