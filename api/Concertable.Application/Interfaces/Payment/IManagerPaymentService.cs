using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Payment;

public interface IManagerPaymentService
{
    Task<PaymentResponse> PayAsync(ManagerDto payer, ManagerDto payee, decimal amount, int bookingId, string? paymentMethodId = null);
}
