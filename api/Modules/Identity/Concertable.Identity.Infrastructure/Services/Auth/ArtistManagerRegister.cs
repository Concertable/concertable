using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

public class ArtistManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;
    private readonly IStripeAccountService stripeAccountService;

    public ArtistManagerRegister(IdentityDbContext context, IStripeAccountService stripeAccountService)
    {
        this.context = context;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = ArtistManagerEntity.Create(request.Email, passwordHash);
        context.Users.Add(manager);
        await stripeAccountService.AddCustomerAsync(manager);
        await stripeAccountService.AddConnectAccountAsync(manager);
        await context.SaveChangesAsync();
    }
}
