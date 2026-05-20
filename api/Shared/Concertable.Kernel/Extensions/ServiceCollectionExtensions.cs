using Concertable.DataAccess;
using Concertable.Shared.Infrastructure.Background;
using Concertable.Shared.Infrastructure.Events;
using Concertable.Shared.Infrastructure.Services;
using Concertable.Shared.Infrastructure.Services.Geocoding;
using Concertable.Shared.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Concertable.Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IBackgroundTaskRunner, BackgroundTaskRunner>();

        services.AddScoped<IImageService, ImageService>();

        services.AddHttpClient("Geocoding", client =>
        {
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
        });
        services.AddScoped<IGeocodingService, GeocodingService>();

        services.Configure<UrlSettings>(configuration.GetSection("Urls"));
        services.AddScoped<IUriService, UriService>();

        return services;
    }

    public static IServiceCollection AddQueueHostedService(this IServiceCollection services)
    {
        services.AddHostedService<QueueHostedService>();
        return services;
    }
}
