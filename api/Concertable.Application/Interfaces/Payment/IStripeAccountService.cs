using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeAccountService
{
    Task AddCustomerAsync(UserEntity user);
    Task CreateCustomerAsync(UserEntity user);
    Task AddConnectAccountAsync(ManagerEntity manager);
    Task CreateConnectAccountAsync(ManagerEntity manager);
    Task<string> GetOnboardingLinkAsync(string stripeAccountId);
    Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeAccountId);
    Task<string> GetPaymentMethodAsync(string stripeCustomerId);
    Task<string> CreateSetupIntentAsync(string stripeCustomerId);
    Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId);
}
