using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResettables(
        this IServiceCollection services,
        IMockNotificationService notificationService,
        IMockStripePaymentClient stripePaymentClient)
    {
        services.AddSingleton<IResettable>(notificationService);
        services.AddSingleton<IResettable>(stripePaymentClient);
        return services;
    }
}
