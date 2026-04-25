using Concertable.Identity.Infrastructure.Data;
using Concertable.Payment.Contracts;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class CustomerRegister : IUserRegister
{
    private readonly IdentityDbContext context;
    private readonly IPaymentModule paymentModule;

    public CustomerRegister(IdentityDbContext context, IPaymentModule paymentModule)
    {
        this.context = context;
        this.paymentModule = paymentModule;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var user = CustomerEntity.Create(request.Email, passwordHash);
        context.Users.Add(user);
        await paymentModule.ProvisionCustomerAsync(user.Id, request.Email);
        await context.SaveChangesAsync();
    }
}
