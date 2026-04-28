using Concertable.User.Application.Mappers;
using Concertable.User.Infrastructure.Data;
using Concertable.User.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure;

internal class UserModule : IUserModule
{
    private readonly UserDbContext context;
    private readonly IUserRepository userRepository;
    private readonly IUserMapper userMapper;
    private readonly IUserLoader userLoader;
    private readonly IUserRegister userRegister;

    public UserModule(
        UserDbContext context,
        IUserRepository userRepository,
        IUserMapper userMapper,
        IUserLoader userLoader,
        IUserRegister userRegister)
    {
        this.context = context;
        this.userRepository = userRepository;
        this.userMapper = userMapper;
        this.userLoader = userLoader;
        this.userRegister = userRegister;
    }

    public async Task<IUser?> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user is null ? null : userMapper.ToDto(user);
    }

    public async Task<IReadOnlyCollection<IUser>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        userMapper.ToDtos(await userRepository.GetByIdsAsync(ids)).ToList();

    public async Task<ManagerDto?> GetManagerByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user is ManagerEntity manager ? manager.ToDto() : null;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        context.Users.AnyAsync(u => u.Email == email, ct);

    public Task CreateAsync(string email, string passwordHash, Role role, CancellationToken ct = default) =>
        userRegister.RegisterAsync(email, passwordHash, role);

    public async Task<UserCredentials?> GetCredentialsByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return user is null ? null : new UserCredentials(user.Id, user.Email, user.PasswordHash, user.IsEmailVerified, user.Role);
    }

    public async Task<UserCredentials?> GetCredentialsByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : new UserCredentials(user.Id, user.Email, user.PasswordHash, user.IsEmailVerified, user.Role);
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
