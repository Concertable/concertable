using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResettables(this IServiceCollection services)
    {
        services.AddSingleton<IResettable>(p => p.GetRequiredService<IMockNotificationService>());
        services.AddSingleton<IResettable>(p => p.GetRequiredService<IMockStripePaymentClient>());
        return services;
    }
}
