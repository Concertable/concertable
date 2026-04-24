using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure.Data;

public interface IRatingProjectionConfigurationProvider
{
    void Configure(ModelBuilder modelBuilder);
}
