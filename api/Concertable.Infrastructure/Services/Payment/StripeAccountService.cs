using Concertable.Application.Interfaces.Payment;
using Concertable.Application.DTOs;
using Concertable.Shared.Exceptions;
using Concertable.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Concertable.Core.Enums;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

public class StripeAccountService : IStripeAccountService
{
    private readonly string baseUri;
    private readonly AccountService accountService;
    private readonly AccountLinkService accountLinkService;
    private readonly CustomerService customerService;
    private readonly PaymentMethodService paymentMethodService;
    private readonly SetupIntentService setupIntentService;

    public StripeAccountService(
        IOptions<StripeSettings> stripeSettings,
        IConfiguration configuration,
        AccountService accountService,
        AccountLinkService accountLinkService,
        CustomerService customerService,
        PaymentMethodService paymentMethodService,
        SetupIntentService setupIntentService)
    {
        StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
        this.baseUri = configuration["BaseUri:http"]!;
        this.accountService = accountService;
        this.accountLinkService = accountLinkService;
        this.customerService = customerService;
        this.paymentMethodService = paymentMethodService;
        this.setupIntentService = setupIntentService;
    }

    public async Task AddCustomerAsync(UserEntity user)
    {
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
        });
        user.StripeCustomerId = customer.Id;
    }

    public async Task AddConnectAccountAsync(ManagerEntity manager)
    {
        var account = await accountService.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Email = manager.Email,
            Country = "GB",
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            }
        });
        manager.StripeAccountId = account.Id;
    }

    public async Task<string> CreateCustomerAsync(string email)
    {
        var customer = await customerService.CreateAsync(new CustomerCreateOptions { Email = email });
        return customer.Id;
    }

    public async Task<string> CreateConnectAccountAsync(string email)
    {
        var account = await accountService.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Email = email,
            Country = "GB",
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            }
        });
        return account.Id;
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

    public async Task<string> GetPaymentMethodAsync(string stripeCustomerId)
    {
        var paymentMethods = await paymentMethodService.ListAsync(new PaymentMethodListOptions
        {
            Customer = stripeCustomerId,
            Type = "card"
        });

        return paymentMethods.FirstOrDefault()?.Id
            ?? throw new NotFoundException($"No payment method found for customer {stripeCustomerId}");
    }

    public async Task<string> CreateSetupIntentAsync(string stripeCustomerId)
    {
        var intent = await setupIntentService.CreateAsync(new SetupIntentCreateOptions
        {
            Customer = stripeCustomerId,
            PaymentMethodTypes = ["card"],
            Usage = "off_session",
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
