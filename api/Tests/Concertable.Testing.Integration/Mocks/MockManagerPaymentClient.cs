using Concertable.Payment.Client;
using Concertable.Payment.Domain;
using FluentResults;

namespace Concertable.Testing.Integration.Mocks;

internal class MockManagerPaymentClient : IManagerPaymentClient
{
    public Task<Result<PaymentResponse>> PayAsync(Guid payerId, Guid payeeId, decimal amount, string paymentMethodId, PaymentSession session, int bookingId, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok(new PaymentResponse { RequiresAction = false, TransactionId = "pi_mock_pay" }));

    public Task<CheckoutSession> CreateSetupSessionAsync(Guid payerId, IDictionary<string, string> metadata, CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("seti_mock_secret", "cuss_mock_secret", "cus_mock"));

    public Task<CheckoutSession> CreateVerifySessionAsync(Guid payerId, IDictionary<string, string> metadata, CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("pi_mock_verify_secret", "cuss_mock_secret", "cus_mock"));

    public Task<CheckoutSession> CreateHoldSessionAsync(Guid payerId, decimal amount, IDictionary<string, string> metadata, CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("pi_mock_hold_secret", "cuss_mock_secret", "cus_mock"));

    public Task<string> FindHeldIntentAsync(Guid payerId, int applicationId, CancellationToken ct = default) =>
        Task.FromResult("pi_mock_held");
}
