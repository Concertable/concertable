using Concertable.Payment.Application.DTOs;

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

    public Task<string?> TryGetSavedPaymentMethodAsync(string? stripeCustomerId) =>
        Task.FromResult<string?>(stripeCustomerId is null ? null : "pm_fake_card");

    public Task<string> CreateSetupIntentAsync(string stripeCustomerId) =>
        Task.FromResult("seti_fake_secret");

    public Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId) =>
        Task.FromResult<PaymentMethodDto?>(new PaymentMethodDto("visa", "4242", 12, 2030));
}
