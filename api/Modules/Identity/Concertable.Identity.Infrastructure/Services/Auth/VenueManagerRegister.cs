using Concertable.Identity.Infrastructure.Data;
using Concertable.Payment.Contracts;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class VenueManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;
    private readonly IPaymentModule paymentModule;

    public VenueManagerRegister(IdentityDbContext context, IPaymentModule paymentModule)
    {
        this.context = context;
        this.paymentModule = paymentModule;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = VenueManagerEntity.Create(request.Email, passwordHash);
        context.Users.Add(manager);
        await paymentModule.ProvisionCustomerAsync(manager.Id, request.Email);
        await paymentModule.ProvisionConnectAccountAsync(manager.Id, request.Email);
        await context.SaveChangesAsync();
    }
}
