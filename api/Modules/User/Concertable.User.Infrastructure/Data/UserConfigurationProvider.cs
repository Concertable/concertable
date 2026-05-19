using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Data.Configurations;
using Concertable.User.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Data;

internal sealed class UserConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new VenueManagerProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ArtistManagerProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AdminProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmailVerificationTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenEntityConfiguration());
    }
}
