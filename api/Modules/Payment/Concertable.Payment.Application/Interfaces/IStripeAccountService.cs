using Concertable.Payment.Application.DTOs;

namespace Concertable.Payment.Application.Interfaces;

internal interface IStripeAccountService
{
    Task ProvisionCustomerAsync(Guid userId, string email, CancellationToken ct = default);
    Task ProvisionConnectAccountAsync(Guid userId, string email, CancellationToken ct = default);
    Task<string> GetOnboardingLinkAsync(string stripeAccountId);
    Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeAccountId);
    Task<string?> TryGetSavedPaymentMethodAsync(string? stripeCustomerId);
    Task<string> CreateSetupIntentAsync(string? stripeCustomerId);
    Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId);
}
