using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Infrastructure.Services.Auth;

public class VenueManagerRegister : IUserRegister
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
        context.Users.Add(manager);
        await stripeAccountService.AddCustomerAsync(manager);
        await stripeAccountService.AddConnectAccountAsync(manager);
        await context.SaveChangesAsync();
    }
}
