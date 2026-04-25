using Concertable.Contract.Abstractions;
using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IPaymentModule
{
    Task<Result<PaymentResponse>> PurchaseTicketsAsync(
        int concertId,
        int quantity,
        string? paymentMethodId,
        decimal price,
        Guid customerUserId,
        IContract contract,
        CancellationToken ct = default);

    Task<Result<PaymentResponse>> SettleBookingAsync(
        int bookingId,
        Guid payerUserId,
        Guid payeeUserId,
        decimal amount,
        IContract contract,
        CancellationToken ct = default);
}
