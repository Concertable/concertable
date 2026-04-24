using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Data;

internal sealed class IdentityConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ManagerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmailVerificationTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenEntityConfiguration());
    }
}
