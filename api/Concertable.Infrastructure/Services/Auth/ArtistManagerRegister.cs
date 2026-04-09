using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Services.Auth;

public class ArtistManagerRegister : IUserRegister
{
    private readonly ApplicationDbContext context;
    private readonly IStripeAccountService stripeAccountService;

    public ArtistManagerRegister(ApplicationDbContext context, IStripeAccountService stripeAccountService)
    {
        this.context = context;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = new ArtistManagerEntity { Email = request.Email, Role = Role.ArtistManager, PasswordHash = passwordHash };
        context.Users.Add(manager);
        await context.SaveChangesAsync();

        manager.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(manager);
        manager.StripeAccountId = await stripeAccountService.CreateConnectAccountAsync(manager);
        await context.SaveChangesAsync();
    }
}
