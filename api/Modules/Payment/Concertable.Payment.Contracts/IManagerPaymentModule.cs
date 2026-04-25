using Concertable.Identity.Contracts;

namespace Concertable.Payment.Contracts;

public interface IManagerPaymentModule
{
    Task<PaymentResponse> PayAsync(
        ManagerDto payer,
        ManagerDto payee,
        decimal amount,
        int bookingId,
        string? paymentMethodId = null);
}
