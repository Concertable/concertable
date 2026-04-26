using Concertable.Messaging.Api.Controllers;
using Concertable.Messaging.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Messaging.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessagingApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessagingModule(configuration);
        services.AddControllers()
            .AddApplicationPart(typeof(MessageController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
