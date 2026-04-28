using Concertable.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Services;

internal class AuthUserSeam : IAuthUserSeam
{
    private readonly IdentityDbContext context;
    private readonly IUserRegister userRegister;
    private readonly IUserLoader userLoader;
    private readonly IUserMapper userMapper;

    public AuthUserSeam(
        IdentityDbContext context,
        IUserRegister userRegister,
        IUserLoader userLoader,
        IUserMapper userMapper)
    {
        this.context = context;
        this.userRegister = userRegister;
        this.userLoader = userLoader;
        this.userMapper = userMapper;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        context.Users.AnyAsync(u => u.Email == email, ct);

    public Task CreateUserAsync(string email, string passwordHash, Role role, CancellationToken ct = default) =>
        userRegister.RegisterAsync(email, passwordHash, role);

    public async Task<AuthUserCredentials?> GetCredentialsByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return user is null ? null : new AuthUserCredentials(user.Id, user.Email, user.PasswordHash, user.IsEmailVerified, user.Role);
    }

    public async Task<AuthUserCredentials?> GetCredentialsByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : new AuthUserCredentials(user.Id, user.Email, user.PasswordHash, user.IsEmailVerified, user.Role);
    }

    public async Task<IUser?> GetUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return null;
        var fullUser = await userLoader.LoadAsync(user);
        return userMapper.ToDto(fullUser);
    }

    public async Task SetEmailVerifiedAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.VerifyEmail();
        await context.SaveChangesAsync(ct);
    }

    public async Task SetPasswordHashAsync(Guid userId, string newHash, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.PasswordHash = newHash;
        await context.SaveChangesAsync(ct);
    }
}
