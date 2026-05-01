using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Data;

internal class UserDbContext(
    DbContextOptions<UserDbContext> options,
    UserConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens => Set<EmailVerificationTokenEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);

        provider.Configure(modelBuilder);
    }
}
