using Concertable.Testing.Integration.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Testing.Integration;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddResettables(
        this IServiceCollection services,
        IMockNotificationService notificationService,
        IMockStripeApiClient stripePaymentClient,
        IMockEmailSender emailSender)
    {
        services.AddSingleton<IResettable>(notificationService);
        services.AddSingleton<IResettable>(stripePaymentClient);
        services.AddSingleton<IResettable>(emailSender);
        return services;
    }
}
