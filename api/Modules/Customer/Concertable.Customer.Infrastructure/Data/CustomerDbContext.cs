using Microsoft.EntityFrameworkCore;
using SharedSchema = Concertable.Data.Infrastructure.Schema;

namespace Concertable.Customer.Infrastructure.Data;

internal class CustomerDbContext(
    DbContextOptions<CustomerDbContext> options,
    CustomerConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<PreferenceEntity> Preferences => Set<PreferenceEntity>();
    public DbSet<GenrePreferenceEntity> GenrePreferences => Set<GenrePreferenceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);

        provider.Configure(modelBuilder);

        modelBuilder.Entity<GenreEntity>().ToTable("Genres", SharedSchema.Name, t => t.ExcludeFromMigrations());
    }
}
