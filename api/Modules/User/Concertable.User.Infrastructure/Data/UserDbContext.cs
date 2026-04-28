using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.User.Infrastructure.Data;

internal class UserDbContext(
    DbContextOptions<UserDbContext> options,
    UserConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);

        provider.Configure(modelBuilder);
    }
}
