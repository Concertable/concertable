using Concertable.Core.Entities;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data.Configurations;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options, IDomainEventDispatcher? dispatcher = null)
    : DbContextBase(options, dispatcher)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens => Set<EmailVerificationTokenEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ManagerEntityConfiguration());

        modelBuilder.Entity<RefreshTokenEntity>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmailVerificationTokenEntity>()
            .HasOne(t => t.User)
            .WithMany(u => u.EmailVerificationTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PasswordResetTokenEntity>()
            .HasOne(t => t.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
