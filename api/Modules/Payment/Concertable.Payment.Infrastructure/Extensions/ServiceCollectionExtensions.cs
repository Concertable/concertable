using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Step 1 scaffold — services land in Step 7 (Move Infrastructure layer to Payment.Infrastructure).
        // Tracked: PAYMENT_MODULE_REFACTOR.md.
        return services;
    }

    public static IServiceCollection AddPaymentDevSeeder(this IServiceCollection services)
    {
        // PaymentDevSeeder lands in Step 9.
        return services;
    }

    public static IServiceCollection AddPaymentTestSeeder(this IServiceCollection services)
    {
        // PaymentTestSeeder lands in Step 9.
        return services;
    }
}
