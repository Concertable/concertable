using Concertable.Payment.Application.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Core.Enums;

namespace Concertable.Infrastructure.Services.Payment;

internal class FakeStripeAccountService : IStripeAccountService
{
    public Task AddCustomerAsync(UserEntity user)
    {
        user.StripeCustomerId = $"cus_fake_{user.Id}";
        return Task.CompletedTask;
    }

    public Task AddConnectAccountAsync(ManagerEntity manager)
    {
        manager.StripeAccountId = $"acct_fake_{manager.Id}";
        return Task.CompletedTask;
    }

    public Task<string> CreateCustomerAsync(string email) =>
        Task.FromResult($"cus_fake_{Guid.NewGuid()}");

    public Task<string> CreateConnectAccountAsync(string email) =>
        Task.FromResult($"acct_fake_{Guid.NewGuid()}");

    public Task<string> GetOnboardingLinkAsync(string stripeId) =>
        Task.FromResult("https://fake-stripe-onboarding.local");

    public Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeId) =>
        Task.FromResult(PayoutAccountStatus.Verified);

    public Task<string> GetPaymentMethodAsync(string stripeId) =>
        Task.FromResult("pm_fake_card");

    public Task<string> CreateSetupIntentAsync(string stripeCustomerId) =>
        Task.FromResult("seti_fake_secret");

    public Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId) =>
        Task.FromResult<PaymentMethodDto?>(new PaymentMethodDto("visa", "4242", 12, 2030));
}
