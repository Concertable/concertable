using Concertable.Payment.Client;
using Concertable.Payment.Domain;
using FluentResults;

namespace Concertable.Testing.Integration.Mocks;

internal class MockEscrowClient : IEscrowClient
{
    public Task<Result<EscrowResponse>> DepositAsync(Guid payerId, Guid payeeId, decimal amount, string paymentMethodId, PaymentSession session, int bookingId, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok(new EscrowResponse(bookingId, "ch_mock", EscrowStatus.Held)));

    public Task<Result<EscrowResponse>> CaptureAsync(Guid payerId, Guid payeeId, decimal amount, string paymentIntentId, int bookingId, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok(new EscrowResponse(bookingId, "ch_mock_captured", EscrowStatus.Held)));

    public Task<Result<TransferResponse?>> ReleaseByBookingIdAsync(int bookingId, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok<TransferResponse?>(new TransferResponse("tr_mock")));
}
