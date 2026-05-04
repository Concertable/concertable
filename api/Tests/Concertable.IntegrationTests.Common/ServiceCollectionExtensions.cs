using Concertable.IntegrationTests.Common.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.IntegrationTests.Common;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddResettables(
        this IServiceCollection services,
        IMockNotificationService notificationService,
        IMockStripeApiClient stripePaymentClient,
        IMockEmailService emailService)
    {
        services.AddSingleton<IResettable>(notificationService);
        services.AddSingleton<IResettable>(stripePaymentClient);
        services.AddSingleton<IResettable>(emailService);
        return services;
    }
}
