using Concertable.Payment.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class CustomerRegister : IUserRegister
{
    private readonly IdentityDbContext context;
    private readonly IStripeAccountService stripeAccountService;

    public CustomerRegister(IdentityDbContext context, IStripeAccountService stripeAccountService)
    {
        this.context = context;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var user = CustomerEntity.Create(request.Email, passwordHash);
        user.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(request.Email);
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
