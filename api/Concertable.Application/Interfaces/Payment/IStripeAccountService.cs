using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeAccountService
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
