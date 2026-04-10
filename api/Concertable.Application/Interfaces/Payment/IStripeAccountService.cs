using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeAccountService
{
    Task AddCustomerAsync(UserEntity user);
    Task CreateCustomerAsync(UserEntity user);
    Task AddConnectAccountAsync(ManagerEntity manager);
    Task CreateConnectAccountAsync(ManagerEntity manager);
    Task<string> GetOnboardingLinkAsync(string stripeAccountId);
    Task<bool> IsUserVerifiedAsync(string stripeAccountId);
    Task<string> GetPaymentMethodAsync(string stripeCustomerId);
    Task<string> CreateSetupIntentAsync(string stripeCustomerId);
    Task<PaymentMethodResponse?> GetPaymentMethodDetailsAsync(string stripeCustomerId);
}
