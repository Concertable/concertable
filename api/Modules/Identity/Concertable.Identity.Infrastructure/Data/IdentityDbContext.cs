using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContextBase(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens => Set<EmailVerificationTokenEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);

        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ManagerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmailVerificationTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenEntityConfiguration());
    }
}
