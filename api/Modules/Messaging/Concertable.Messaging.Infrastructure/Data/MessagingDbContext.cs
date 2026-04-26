using Microsoft.EntityFrameworkCore;

namespace Concertable.Messaging.Infrastructure.Data;

internal class MessagingDbContext(
    DbContextOptions<MessagingDbContext> options,
    MessagingConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
