using Concertable.Auth.Infrastructure.Data.Configurations;
using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Auth.Infrastructure.Data;

internal class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : DbContextBase(options)
{
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens => Set<EmailVerificationTokenEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);
        modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmailVerificationTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenEntityConfiguration());
    }
}
