using Concertable.Seed.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Seed.Extensions;

public static class SeedingServiceCollectionExtensions
{
    public static IServiceCollection AddSeedingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<SeedingScope>();
        services.AddSingleton<SeedingIdentityInterceptor>();
        return services;
    }
}
