namespace Concertable.Payment.Application.Interfaces;

internal interface IManagerPaymentService
{
    Task<PaymentResponse> PayAsync(ManagerDto payer, ManagerDto payee, decimal amount, int bookingId, string? paymentMethodId = null);
}
