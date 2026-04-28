using Concertable.Identity.Api.Controllers;
using Concertable.Identity.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Identity.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityModule(configuration);
        services.AddControllers()
            .AddApplicationPart(typeof(UserController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
