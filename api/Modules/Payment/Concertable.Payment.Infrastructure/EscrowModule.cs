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

    public Task<Result<EscrowResponse>> HoldAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        CancellationToken ct = default) =>
        escrowService.HoldAsync(payerId, payeeId, amount, paymentMethodId, session, bookingId, ct);

    public Task<Result<EscrowResponse>> CaptureHoldAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentIntentId,
        int bookingId,
        CancellationToken ct = default) =>
        escrowService.CaptureHoldAsync(payerId, payeeId, amount, paymentIntentId, bookingId, ct);

    public Task<Result<TransferResponse?>> ReleaseByBookingIdAsync(int bookingId, CancellationToken ct = default) =>
        escrowService.ReleaseByBookingIdAsync(bookingId, ct);
}
