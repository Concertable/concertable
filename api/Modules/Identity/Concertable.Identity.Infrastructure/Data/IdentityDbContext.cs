using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Identity.Infrastructure.Data;

internal class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    IdentityConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);

        provider.Configure(modelBuilder);
    }
}
