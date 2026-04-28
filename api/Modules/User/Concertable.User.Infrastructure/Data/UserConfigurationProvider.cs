using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Data;

internal sealed class UserConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ManagerEntityConfiguration());
    }
}
