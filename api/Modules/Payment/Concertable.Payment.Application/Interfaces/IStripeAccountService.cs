using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Contracts;

namespace Concertable.Payment.Application.Interfaces;

internal interface IStripeAccountService
{
    Task ProvisionCustomerAsync(Guid userId, string email, CancellationToken ct = default);
    Task ProvisionConnectAccountAsync(Guid userId, string email, CancellationToken ct = default);
    Task<string> GetOnboardingLinkAsync(string stripeAccountId);
    Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeAccountId);
    Task<string> CreateSetupIntentAsync(string? stripeCustomerId);
    Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId);

    Task<CheckoutSession> CreatePaymentSessionAsync(
        string stripeCustomerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task<CheckoutSession> CreateSetupSessionAsync(
        string stripeCustomerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task VerifyAndVoidAsync(
        string stripeCustomerId,
        string paymentMethodId,
        CancellationToken ct = default);
}
