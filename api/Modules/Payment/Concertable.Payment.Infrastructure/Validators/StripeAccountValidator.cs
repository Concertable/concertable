namespace Concertable.Payment.Infrastructure.Validators;

internal class StripeAccountValidator : IStripeValidationStrategy
{
    private readonly ICurrentUser currentUser;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;

    public StripeAccountValidator(ICurrentUser currentUser, IStripeAccountService stripeAccountService, IPayoutAccountRepository payoutAccountRepository)
    {
        this.currentUser = currentUser;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
    }

    public async Task<bool> ValidateAsync()
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(currentUser.GetId());
        if (account?.StripeAccountId is null) return false;

        return await stripeAccountService.GetAccountStatusAsync(account.StripeAccountId) == PayoutAccountStatus.Verified;
    }
}
