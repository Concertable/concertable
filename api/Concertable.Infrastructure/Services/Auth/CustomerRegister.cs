using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Services.Auth;

public class CustomerRegister : IUserRegister
{
    private readonly ApplicationDbContext context;
    private readonly IStripeAccountService stripeAccountService;

    public CustomerRegister(ApplicationDbContext context, IStripeAccountService stripeAccountService)
    {
        this.context = context;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var user = new CustomerEntity { Email = request.Email, Role = Role.Customer, PasswordHash = passwordHash };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        user.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(user);
        await context.SaveChangesAsync();
    }
}
