using Concertable.Search.Api.Controllers;
using Concertable.Search.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Search.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchApi(this IServiceCollection services)
    {
        services.AddSearchModule();
        services.AddControllers()
            .AddApplicationPart(typeof(AutocompleteController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
