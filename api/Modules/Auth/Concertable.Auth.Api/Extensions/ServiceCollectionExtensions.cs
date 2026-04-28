using Concertable.Auth.Api.Controllers;
using Concertable.Auth.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Auth.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthModule(configuration);
        services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
