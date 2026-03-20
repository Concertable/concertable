using Application.Interfaces;
using Application.Interfaces.Payment;
using Core.Entities;
using Core.Exceptions;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Services.Payment;

public class StripeAccountService : IStripeAccountService
{
    private readonly StripeSettings stripeSettings;
    private readonly IUserRepository userRepository;
    private readonly string baseUri;

    public StripeAccountService(
        IUserRepository userRepository,
        IOptions<StripeSettings> stripeSettings,
        IConfiguration configuration)
    {
        this.stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = this.stripeSettings.SecretKey;
        this.userRepository = userRepository;
        this.baseUri = configuration["BaseUri:http"]!;
    }

    public async Task<string> CreateStripeAccountAsync(UserEntity user)
    {
        var accountService = new Stripe.AccountService();
        var accountOptions = new AccountCreateOptions
        {
            Type = "express", // Simplest form of account creation, where delegate the KYC to stripe entirely
            Email = user.Email,
            Country = "GB",
            Capabilities = new AccountCapabilitiesOptions //Make sure user is capable of accepting payments and transferring funds
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            }
        };

        var account = await accountService.CreateAsync(accountOptions);

        user.StripeId = account.Id;
        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return account.Id;
    }

    public async Task<string> GetOnboardingLinkAsync(string stripeId)
    {
        var service = new AccountLinkService();

        var options = new AccountLinkCreateOptions
        {
            Account = stripeId,
            RefreshUrl = $"{baseUri}/fail",
            ReturnUrl = $"{baseUri}/success",
            Type = "account_onboarding"
        };

        var link = await service.CreateAsync(options);
        return link.Url;
    }

    public async Task<bool> IsUserVerifiedAsync(string stripeId)
    {
        var service = new Stripe.AccountService();
        var account = await service.GetAsync(stripeId);
        return account.PayoutsEnabled && account.ChargesEnabled;
    }

    public async Task<string> GetPaymentMethodAsync(string stripeId)
    {
        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(new PaymentMethodListOptions
        {
            Customer = stripeId,
            Type = "card"
        });

        var paymentMethod = paymentMethods.FirstOrDefault()
            ?? throw new NotFoundException($"No payment method found for Stripe account {stripeId}");

        return paymentMethod.Id;
    }
}
