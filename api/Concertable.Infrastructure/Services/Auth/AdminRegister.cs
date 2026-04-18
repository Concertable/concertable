using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Services.Auth;

public class AdminRegister : IUserRegister
{
    private readonly ApplicationDbContext context;

    public AdminRegister(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(RegisterRequest request, string passwordHash)
    {
        var admin = AdminEntity.Create(request.Email, passwordHash);
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
