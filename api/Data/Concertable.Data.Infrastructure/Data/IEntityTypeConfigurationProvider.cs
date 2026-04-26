using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure.Data;

public interface IEntityTypeConfigurationProvider
{
    void Configure(ModelBuilder modelBuilder);
}
