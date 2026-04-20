using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class VenueManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;
    private readonly IStripeAccountService stripeAccountService;

    public VenueManagerRegister(IdentityDbContext context, IStripeAccountService stripeAccountService)
    {
        this.context = context;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = VenueManagerEntity.Create(request.Email, passwordHash);
        manager.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(request.Email);
        manager.StripeAccountId = await stripeAccountService.CreateConnectAccountAsync(request.Email);
        context.Users.Add(manager);
        await context.SaveChangesAsync();
    }
}
