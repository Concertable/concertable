using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Contracts;

namespace Concertable.Payment.Infrastructure.Services;

internal class FakeStripeAccountService : IStripeAccountService
{
    public Task ProvisionCustomerAsync(Guid userId, string email, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task ProvisionConnectAccountAsync(Guid userId, string email, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<string> GetOnboardingLinkAsync(string stripeId) =>
        Task.FromResult("https://fake-stripe-onboarding.local");

    public Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeId) =>
        Task.FromResult(PayoutAccountStatus.Verified);

    public Task<string?> TryGetPaymentMethodAsync(string? stripeCustomerId) =>
        Task.FromResult<string?>(stripeCustomerId is null ? null : "pm_fake_card");

    public Task<string> CreateSetupIntentAsync(string? stripeCustomerId) =>
        Task.FromResult("seti_fake_secret");

    public Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId) =>
        Task.FromResult<PaymentMethodDto?>(new PaymentMethodDto("visa", "4242", 12, 2030));

    public Task<CheckoutSession> CreatePaymentSessionAsync(
        string stripeCustomerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("pi_fake_secret", "cuss_fake_secret", stripeCustomerId));

    public Task<CheckoutSession> CreateSavedCardSessionAsync(
        string stripeCustomerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("seti_fake_secret", "cuss_fake_secret", stripeCustomerId));
}
