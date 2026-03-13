using Core.Entities;

namespace Application.Interfaces;

public interface IStripeAccountService
{
    Task<string> CreateStripeAccountAsync(User user);
    Task<string> GetOnboardingLinkAsync(string stripeId);
    Task<bool> IsUserVerifiedAsync(string stripeId);
}
