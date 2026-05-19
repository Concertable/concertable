using Concertable.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Concertable.User.Infrastructure;

internal class UserModule : IUserModule
{
    private readonly UserDbContext context;
    private readonly IUserRepository userRepository;
    private readonly IUserRegister userRegister;

    public UserModule(
        UserDbContext context,
        IUserRepository userRepository,
        IUserRegister userRegister)
    {
        this.context = context;
        this.userRepository = userRepository;
        this.userRegister = userRegister;
    }

    public async Task<IUser?> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user is null ? null : await MapToUserAsync(user);
    }

    public async Task<IReadOnlyCollection<IUser>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var users = await userRepository.GetByIdsAsync(ids);
        var result = new List<IUser>(users.Count);
        foreach (var user in users)
            result.Add(await MapToUserAsync(user));
        return result;
    }

    public async Task<ManagerDto?> GetManagerByIdAsync(Guid userId)
    {
        var isManager = await context.VenueManagerProfiles.AnyAsync(p => p.Sub == userId)
            || await context.ArtistManagerProfiles.AnyAsync(p => p.Sub == userId)
            || await context.AdminProfiles.AnyAsync(p => p.Sub == userId);

        if (!isManager) return null;

        var user = await userRepository.GetByIdAsync(userId);
        return user is null ? null : new ManagerDto { Id = user.Id, Email = user.Email, Avatar = user.Avatar };
    }

    public Task<bool> EmailExistsAsync(string email, Role role, CancellationToken ct = default) =>
        context.Users.WhereCredentials(email, role).AnyAsync(ct);

    public Task CreateAsync(string email, string passwordHash, Role role, CancellationToken ct = default) =>
        userRegister.RegisterAsync(email, passwordHash, role);

    public async Task<UserCredentials?> GetCredentialsByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await context.Users.Where(u => u.Email == email).FirstOrDefaultAsync(ct);
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

    public async Task<string?> CreateEmailVerificationTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null || user.IsEmailVerified) return null;

        var token = GenerateToken();
        var entity = EmailVerificationTokenEntity.Create(userId, token, DateTime.UtcNow.AddHours(24));
        context.EmailVerificationTokens.Add(entity);
        await context.SaveChangesAsync(ct);
        return token;
    }

    public async Task<bool> VerifyEmailWithTokenAsync(string token, CancellationToken ct = default)
    {
        var entity = await context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
        if (entity is null || !entity.IsActive) return false;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == entity.UserId, ct);
        if (user is null) return false;

        user.VerifyEmail();
        context.EmailVerificationTokens.Remove(entity);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await context.Users.Where(u => u.Email == email).FirstOrDefaultAsync(ct);
        if (user is null) return null;

        var token = GenerateToken();
        var entity = PasswordResetTokenEntity.Create(user.Id, token, DateTime.UtcNow.AddHours(1));
        context.PasswordResetTokens.Add(entity);
        await context.SaveChangesAsync(ct);
        return token;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(string token, string newPasswordHash, CancellationToken ct = default)
    {
        var entity = await context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
        if (entity is null || !entity.IsActive) return false;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == entity.UserId, ct);
        if (user is null) return false;

        user.PasswordHash = newPasswordHash;
        context.PasswordResetTokens.Remove(entity);
        await context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<IUser> MapToUserAsync(UserEntity user) => user.Role switch
    {
        Role.VenueManager => await MapVenueManagerAsync(user),
        Role.ArtistManager => await MapArtistManagerAsync(user),
        Role.Customer => MapCustomer(user),
        _ => MapAdmin(user),
    };

    private async Task<IUser> MapVenueManagerAsync(UserEntity user)
    {
        var profile = await context.VenueManagerProfiles.FindAsync(user.Id);
        return new VenueManagerDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            Latitude = user.Location.ToLatitude(),
            Longitude = user.Location.ToLongitude(),
            County = user.Address?.County,
            Town = user.Address?.Town,
            VenueId = profile?.VenueId,
            IsEmailVerified = user.IsEmailVerified,
        };
    }

    private async Task<IUser> MapArtistManagerAsync(UserEntity user)
    {
        var profile = await context.ArtistManagerProfiles.FindAsync(user.Id);
        return new ArtistManagerDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            Latitude = user.Location.ToLatitude(),
            Longitude = user.Location.ToLongitude(),
            County = user.Address?.County,
            Town = user.Address?.Town,
            ArtistId = profile?.ArtistId,
            IsEmailVerified = user.IsEmailVerified,
        };
    }

    private static IUser MapCustomer(UserEntity user) => new CustomerDto
    {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role,
        Latitude = user.Location.ToLatitude(),
        Longitude = user.Location.ToLongitude(),
        County = user.Address?.County,
        Town = user.Address?.Town,
        IsEmailVerified = user.IsEmailVerified,
    };

    private static IUser MapAdmin(UserEntity user) => new AdminDto
    {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role,
        Latitude = user.Location.ToLatitude(),
        Longitude = user.Location.ToLongitude(),
        County = user.Address?.County,
        Town = user.Address?.Town,
        IsEmailVerified = user.IsEmailVerified,
    };

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
