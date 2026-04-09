using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
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
        var admin = new AdminEntity { Email = request.Email, Role = Role.Admin, PasswordHash = passwordHash };
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
