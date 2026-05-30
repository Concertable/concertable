using Concertable.Seed.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Seed.Extensions;

public static class SeedingDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseSeedingSupport(this DbContextOptionsBuilder builder, IServiceProvider sp)
        => builder.AddInterceptors(sp.GetRequiredService<SeedingIdentityInterceptor>());
}
