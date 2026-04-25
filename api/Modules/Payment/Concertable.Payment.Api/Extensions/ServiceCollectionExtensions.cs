using Concertable.Payment.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPaymentModule(configuration);
        // Controllers + InternalControllerFeatureProvider land in Step 8.
        return services;
    }
}
