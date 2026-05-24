using Concertable.Seeding.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Seeding.Extensions;

public static class SeedingServiceCollectionExtensions
{
    public static IServiceCollection AddSeedingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<SeedingScope>();
        services.AddSingleton<SeedingIdentityInterceptor>();
        return services;
    }
}
