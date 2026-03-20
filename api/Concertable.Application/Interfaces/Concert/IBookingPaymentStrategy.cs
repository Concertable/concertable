using Application.Responses;

namespace Application.Interfaces.Concert;

public interface IBookingPaymentStrategy : IContractStrategy
{
    Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId);
}
