using Concertable.Identity.Domain;
using Concertable.Payment.Application.DTOs;

namespace Concertable.Payment.Application.Interfaces;

// TEMPORARY: AddCustomerAsync/AddConnectAccountAsync still take Identity.Domain entities
// because legacy impls write to UserEntity.StripeCustomerId / ManagerEntity.StripeAccountId.
// Step 7 rewrites both signatures to write PayoutAccountEntity directly; Identity.Domain ref
// retires then.
internal interface IStripeAccountService
{
    Task AddCustomerAsync(UserEntity user);
    Task AddConnectAccountAsync(ManagerEntity manager);
    Task<string> CreateCustomerAsync(string email);
    Task<string> CreateConnectAccountAsync(string email);
    Task<string> GetOnboardingLinkAsync(string stripeAccountId);
    Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeAccountId);
    Task<string> GetPaymentMethodAsync(string stripeCustomerId);
    Task<string> CreateSetupIntentAsync(string stripeCustomerId);
    Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId);
}
