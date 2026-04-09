using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;
using Concertable.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

public class StripeAccountService : IStripeAccountService
{
    private readonly IUserRepository userRepository;
    private readonly string baseUri;
    private readonly AccountService accountService;
    private readonly AccountLinkService accountLinkService;
    private readonly CustomerService customerService;
    private readonly PaymentMethodService paymentMethodService;
    private readonly SetupIntentService setupIntentService;

    public StripeAccountService(
        IUserRepository userRepository,
        IOptions<StripeSettings> stripeSettings,
        IConfiguration configuration,
        AccountService accountService,
        AccountLinkService accountLinkService,
        CustomerService customerService,
        PaymentMethodService paymentMethodService,
        SetupIntentService setupIntentService)
    {
        StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
        this.userRepository = userRepository;
        this.baseUri = configuration["BaseUri:http"]!;
        this.accountService = accountService;
        this.accountLinkService = accountLinkService;
        this.customerService = customerService;
        this.paymentMethodService = paymentMethodService;
        this.setupIntentService = setupIntentService;
    }

    public async Task<string> CreateConnectAccountAsync(ManagerEntity manager)
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
        userRepository.Update(manager);
        await userRepository.SaveChangesAsync();

        return account.Id;
    }

    public async Task<string> CreateCustomerAsync(UserEntity user)
    {
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
        });

        user.StripeCustomerId = customer.Id;
        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return customer.Id;
    }

    public async Task<string> GetOnboardingLinkAsync(string stripeAccountId)
    {
        var link = await accountLinkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = stripeAccountId,
            RefreshUrl = $"{baseUri}/fail",
            ReturnUrl = $"{baseUri}/success",
            Type = "account_onboarding"
        });

        return link.Url;
    }

    public async Task<bool> IsUserVerifiedAsync(string stripeAccountId)
    {
        var account = await accountService.GetAsync(stripeAccountId);
        return account.PayoutsEnabled && account.ChargesEnabled;
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
        });

        return intent.ClientSecret;
    }
}
