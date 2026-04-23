using Concertable.Web.IntegrationTests.Infrastructure.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddResettables(
        this IServiceCollection services,
        IMockNotificationService notificationService,
        IMockStripePaymentClient stripePaymentClient,
        IMockEmailService emailService)
    {
        services.AddSingleton<IResettable>(notificationService);
        services.AddSingleton<IResettable>(stripePaymentClient);
        services.AddSingleton<IResettable>(emailService);
        return services;
    }
}
