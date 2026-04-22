using Concertable.Concert.Api.Controllers;
using Concertable.Concert.Api.Handlers;
using Concertable.Concert.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConcertApi(this IServiceCollection services)
    {
        services.AddConcertModule();
        services.AddScoped<IConcertPostedHandler, ConcertPostedHandler>();
        services.AddControllers()
            .AddApplicationPart(typeof(ConcertController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
