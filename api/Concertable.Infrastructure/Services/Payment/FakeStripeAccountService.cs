using Concertable.Application.Interfaces.Payment;
using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Services.Payment;

public class FakeStripeAccountService : IStripeAccountService
{
    public Task AddCustomerAsync(UserEntity user)
    {
        user.StripeCustomerId = $"cus_fake_{user.Id}";
        return Task.CompletedTask;
    }

    public Task CreateCustomerAsync(UserEntity user) => AddCustomerAsync(user);

    public Task AddConnectAccountAsync(ManagerEntity manager)
    {
        manager.StripeAccountId = $"acct_fake_{manager.Id}";
        return Task.CompletedTask;
    }

    public Task CreateConnectAccountAsync(ManagerEntity manager) => AddConnectAccountAsync(manager);

    public Task<string> GetOnboardingLinkAsync(string stripeId) =>
        Task.FromResult("https://fake-stripe-onboarding.local");

    public Task<bool> IsUserVerifiedAsync(string stripeId) =>
        Task.FromResult(true);

    public Task<string> GetPaymentMethodAsync(string stripeId) =>
        Task.FromResult("pm_fake_card");

    public Task<string> CreateSetupIntentAsync(string stripeCustomerId) =>
        Task.FromResult("seti_fake_secret");

    public Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId) =>
        Task.FromResult<PaymentMethodDto?>(new PaymentMethodDto("visa", "4242", 12, 2030));
}
