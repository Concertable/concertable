using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Services.Payment;

public class FakeStripeAccountService : IStripeAccountService
{
    public Task<string> CreateConnectAccountAsync(ManagerEntity manager) =>
        Task.FromResult($"acct_fake_{manager.Id}");

    public Task<string> CreateCustomerAsync(UserEntity user) =>
        Task.FromResult($"cus_fake_{user.Id}");

    public Task<string> GetOnboardingLinkAsync(string stripeId) =>
        Task.FromResult("https://fake-stripe-onboarding.local");

    public Task<bool> IsUserVerifiedAsync(string stripeId) =>
        Task.FromResult(true);

    public Task<string> GetPaymentMethodAsync(string stripeId) =>
        Task.FromResult("pm_fake_card");

    public Task<string> CreateSetupIntentAsync(string stripeCustomerId) =>
        Task.FromResult("seti_fake_secret");

    public Task<PaymentMethodResponse?> GetPaymentMethodDetailsAsync(string stripeCustomerId) =>
        Task.FromResult<PaymentMethodResponse?>(new PaymentMethodResponse("visa", "4242", 12, 2030));
}
