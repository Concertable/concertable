using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeAccountService
{
    Task<string> CreateStripeAccountAsync(UserEntity user);
    Task<string> GetOnboardingLinkAsync(string stripeId);
    Task<bool> IsUserVerifiedAsync(string stripeId);
    Task<string> GetPaymentMethodAsync(string stripeId);
}
