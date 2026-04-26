using Concertable.Data.Infrastructure.Data;
using Concertable.Messaging.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Messaging.Infrastructure.Data;

internal sealed class MessagingConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MessageEntityConfiguration());
    }
}
