using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeAccountService
{
    Task<string> CreateConnectAccountAsync(ManagerEntity manager);
    Task<string> CreateCustomerAsync(UserEntity user);
    Task<string> GetOnboardingLinkAsync(string stripeAccountId);
    Task<bool> IsUserVerifiedAsync(string stripeAccountId);
    Task<string> GetPaymentMethodAsync(string stripeCustomerId);
    Task<string> CreateSetupIntentAsync(string stripeCustomerId);
    Task<PaymentMethodResponse?> GetPaymentMethodDetailsAsync(string stripeCustomerId);
}
