using Concertable.Payment.Api.Controllers;
using Concertable.Payment.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPaymentModule(configuration);
        services.AddControllers()
            .AddApplicationPart(typeof(PaymentController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
