using Concertable.Auth.Domain;

namespace Concertable.Auth.Application.Interfaces;

internal interface IRefreshTokenRepository : IIdRepository<RefreshTokenEntity>
{
    Task<RefreshTokenEntity?> GetByTokenAsync(string token, CancellationToken ct = default);
}

internal interface IEmailVerificationTokenRepository : IIdRepository<EmailVerificationTokenEntity>
{
    Task<EmailVerificationTokenEntity?> GetByTokenAsync(string token, CancellationToken ct = default);
}

internal interface IPasswordResetTokenRepository : IIdRepository<PasswordResetTokenEntity>
{
    Task<PasswordResetTokenEntity?> GetByTokenAsync(string token, CancellationToken ct = default);
}
