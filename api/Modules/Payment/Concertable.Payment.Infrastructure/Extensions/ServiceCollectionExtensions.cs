using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Concertable.Payment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddSingleton<PaymentConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<PaymentConfigurationProvider>());

        // Services + repositories land in Step 7. Tracked: PAYMENT_MODULE_REFACTOR.md.
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
