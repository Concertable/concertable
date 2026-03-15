using Core.Entities;

namespace Application.Interfaces.Payment;

public interface IStripeAccountService
{
    Task<string> CreateStripeAccountAsync(UserEntity user);
    Task<string> GetOnboardingLinkAsync(string stripeId);
    Task<bool> IsUserVerifiedAsync(string stripeId);
}
