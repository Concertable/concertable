using Application.Interfaces.Payment;
using Core.Entities;

namespace Infrastructure.Services.Payment;

public class FakeStripeAccountService : IStripeAccountService
{
    public Task<string> CreateStripeAccountAsync(UserEntity user) =>
        Task.FromResult($"acct_fake_{user.Id}");

    public Task<string> GetOnboardingLinkAsync(string stripeId) =>
        Task.FromResult("https://fake-stripe-onboarding.local");

    public Task<bool> IsUserVerifiedAsync(string stripeId) =>
        Task.FromResult(true);

    public Task<string> GetPaymentMethodAsync(string stripeId) =>
        Task.FromResult("pm_fake_card");
}
