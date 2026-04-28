using Concertable.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Auth.Infrastructure.Repositories;

internal class RefreshTokenRepository(AuthDbContext context)
    : Repository<RefreshTokenEntity>(context), IRefreshTokenRepository
{
    public Task<RefreshTokenEntity?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);
}

internal class EmailVerificationTokenRepository(AuthDbContext context)
    : Repository<EmailVerificationTokenEntity>(context), IEmailVerificationTokenRepository
{
    public Task<EmailVerificationTokenEntity?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
}

internal class PasswordResetTokenRepository(AuthDbContext context)
    : Repository<PasswordResetTokenEntity>(context), IPasswordResetTokenRepository
{
    public Task<PasswordResetTokenEntity?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
}
