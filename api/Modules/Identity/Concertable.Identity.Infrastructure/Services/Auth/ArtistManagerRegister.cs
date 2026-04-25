using Concertable.Identity.Infrastructure.Data;
using Concertable.Payment.Contracts;

namespace Concertable.Identity.Infrastructure.Services.Auth;

internal class ArtistManagerRegister : IUserRegister
{
    private readonly IdentityDbContext context;
    private readonly IPaymentModule paymentModule;

    public ArtistManagerRegister(IdentityDbContext context, IPaymentModule paymentModule)
    {
        this.context = context;
        this.paymentModule = paymentModule;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var manager = ArtistManagerEntity.Create(request.Email, passwordHash);
        context.Users.Add(manager);
        await paymentModule.ProvisionCustomerAsync(manager.Id, request.Email);
        await paymentModule.ProvisionConnectAccountAsync(manager.Id, request.Email);
        await context.SaveChangesAsync();
    }
}
