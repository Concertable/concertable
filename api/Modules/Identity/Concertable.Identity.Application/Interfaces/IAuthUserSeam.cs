namespace Concertable.Identity.Application.Interfaces;

internal record AuthUserCredentials(Guid Id, string Email, string PasswordHash, bool IsEmailVerified, Role Role);

internal interface IAuthUserSeam
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task CreateUserAsync(string email, string passwordHash, Role role, CancellationToken ct = default);
    Task<AuthUserCredentials?> GetCredentialsByEmailAsync(string email, CancellationToken ct = default);
    Task<AuthUserCredentials?> GetCredentialsByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IUser?> GetUserAsync(Guid userId, CancellationToken ct = default);
    Task SetEmailVerifiedAsync(Guid userId, CancellationToken ct = default);
    Task SetPasswordHashAsync(Guid userId, string newHash, CancellationToken ct = default);
}
