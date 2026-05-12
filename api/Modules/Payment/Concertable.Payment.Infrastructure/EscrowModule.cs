using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Contracts;
using FluentResults;

namespace Concertable.Payment.Infrastructure;

internal class EscrowModule : IEscrowModule
{
    private readonly IEscrowService escrowService;

    public EscrowModule(IEscrowService escrowService)
    {
        this.escrowService = escrowService;
    }

    public Task<Result<EscrowResponse>> DepositAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        CancellationToken ct = default) =>
        escrowService.DepositAsync(payerId, payeeId, amount, paymentMethodId, session, bookingId, ct);

    public Task<Result<EscrowResponse>> CaptureAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentIntentId,
        int bookingId,
        CancellationToken ct = default) =>
        escrowService.CaptureAsync(payerId, payeeId, amount, paymentIntentId, bookingId, ct);

    public Task<Result<TransferResponse?>> ReleaseByBookingIdAsync(int bookingId, CancellationToken ct = default) =>
        escrowService.ReleaseByBookingIdAsync(bookingId, ct);
}
