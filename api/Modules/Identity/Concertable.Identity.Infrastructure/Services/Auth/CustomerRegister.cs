using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
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
        var user = CustomerEntity.Create(request.Email, passwordHash);
        context.Users.Add(user);
        await stripeAccountService.AddCustomerAsync(user);
        await context.SaveChangesAsync();
    }
}
