using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Infrastructure.Settings;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services;

internal class StripeAccountService : IStripeAccountService
{
    private readonly string baseUri;
    private readonly AccountService accountService;
    private readonly AccountLinkService accountLinkService;
    private readonly CustomerService customerService;
    private readonly PaymentMethodService paymentMethodService;
    private readonly SetupIntentService setupIntentService;
    private readonly IPayoutAccountRepository payoutAccountRepository;

    public StripeAccountService(
        IOptions<StripeSettings> stripeSettings,
        IConfiguration configuration,
        AccountService accountService,
        AccountLinkService accountLinkService,
        CustomerService customerService,
        PaymentMethodService paymentMethodService,
        SetupIntentService setupIntentService,
        IPayoutAccountRepository payoutAccountRepository)
    {
        StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
        this.baseUri = configuration["BaseUri:http"]!;
        this.accountService = accountService;
        this.accountLinkService = accountLinkService;
        this.customerService = customerService;
        this.paymentMethodService = paymentMethodService;
        this.setupIntentService = setupIntentService;
        this.payoutAccountRepository = payoutAccountRepository;
    }

    public async Task ProvisionCustomerAsync(Guid userId, string email, CancellationToken ct = default)
    {
        var customer = await customerService.CreateAsync(new CustomerCreateOptions { Email = email }, cancellationToken: ct);
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct) ?? PayoutAccountEntity.Create(userId);
        account.LinkCustomer(customer.Id);
        if (account.Id == 0)
            await payoutAccountRepository.AddAsync(account, ct);
        await payoutAccountRepository.SaveChangesAsync(ct);
    }

    public async Task ProvisionConnectAccountAsync(Guid userId, string email, CancellationToken ct = default)
    {
        var stripeAccount = await accountService.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Email = email,
            Country = "GB",
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            }
        }, cancellationToken: ct);
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct) ?? PayoutAccountEntity.Create(userId);
        account.LinkAccount(stripeAccount.Id);
        if (account.Id == 0)
            await payoutAccountRepository.AddAsync(account, ct);
        await payoutAccountRepository.SaveChangesAsync(ct);
    }

    public async Task<string> GetOnboardingLinkAsync(string stripeAccountId)
    {
        var link = await accountLinkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = stripeAccountId,
            RefreshUrl = $"{baseUri}/stripe-refresh",
            ReturnUrl = $"{baseUri}/stripe-return",
            Type = "account_onboarding"
        });

        return link.Url;
    }

    public async Task<PayoutAccountStatus> GetAccountStatusAsync(string stripeAccountId)
    {
        var account = await accountService.GetAsync(stripeAccountId);

        if (account.ChargesEnabled && account.PayoutsEnabled)
            return PayoutAccountStatus.Verified;

        if (account.Requirements?.CurrentlyDue?.Any() == true ||
            account.Requirements?.PendingVerification?.Any() == true)
            return PayoutAccountStatus.Pending;

        return PayoutAccountStatus.NotVerified;
    }

    public async Task<string?> TryGetPaymentMethodAsync(string? stripeCustomerId)
    {
        if (stripeCustomerId is null) return null;

        var paymentMethods = await paymentMethodService.ListAsync(new PaymentMethodListOptions
        {
            Customer = stripeCustomerId,
            Type = "card"
        });

        return paymentMethods.FirstOrDefault()?.Id;
    }

    public async Task<string> CreateSetupIntentAsync(string? stripeCustomerId)
    {
        var intent = await setupIntentService.CreateAsync(new SetupIntentCreateOptions
        {
            Customer = stripeCustomerId,
            PaymentMethodTypes = ["card"],
            Usage = stripeCustomerId is null ? "on_session" : "off_session",
        });

        return intent.ClientSecret;
    }

    public async Task<PaymentMethodDto?> GetPaymentMethodDetailsAsync(string stripeCustomerId)
    {
        var paymentMethods = await paymentMethodService.ListAsync(new PaymentMethodListOptions
        {
            Customer = stripeCustomerId,
            Type = "card"
        });

        var card = paymentMethods.FirstOrDefault()?.Card;
        if (card is null) return null;

        return new PaymentMethodDto(card.Brand, card.Last4, (int)card.ExpMonth, (int)card.ExpYear);
    }

}
