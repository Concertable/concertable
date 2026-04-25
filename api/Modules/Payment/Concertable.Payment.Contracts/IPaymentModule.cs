using Concertable.Contract.Abstractions;
using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IPaymentModule
{
    Task ProvisionCustomerAsync(Guid userId, string email, CancellationToken ct = default);
    Task ProvisionConnectAccountAsync(Guid userId, string email, CancellationToken ct = default);

    Task<Result<PaymentResponse>> PurchaseTicketsAsync(
        int concertId,
        int quantity,
        string? paymentMethodId,
        decimal price,
        Guid customerUserId,
        Guid payeeUserId,
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
