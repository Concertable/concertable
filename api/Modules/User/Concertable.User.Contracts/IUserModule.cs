namespace Concertable.User.Contracts;

public record UserCredentials(Guid Id, string Email, string PasswordHash, bool IsEmailVerified, Role Role);

public interface IUserModule
{
    Task<IUser?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<IUser>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<ManagerDto?> GetManagerByIdAsync(Guid userId);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task CreateAsync(string email, string passwordHash, Role role, CancellationToken ct = default);
    Task<UserCredentials?> GetCredentialsByEmailAsync(string email, CancellationToken ct = default);
    Task<UserCredentials?> GetCredentialsByIdAsync(Guid userId, CancellationToken ct = default);
    Task SetEmailVerifiedAsync(Guid userId, CancellationToken ct = default);
    Task SetPasswordHashAsync(Guid userId, string newHash, CancellationToken ct = default);

    Task<string?> CreateEmailVerificationTokenAsync(Guid userId, CancellationToken ct = default);
    Task<bool> VerifyEmailWithTokenAsync(string token, CancellationToken ct = default);
    Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken ct = default);
    Task<bool> ResetPasswordWithTokenAsync(string token, string newPasswordHash, CancellationToken ct = default);
}
