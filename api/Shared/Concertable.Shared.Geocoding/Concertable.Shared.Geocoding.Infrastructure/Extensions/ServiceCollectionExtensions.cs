using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Geocoding.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedGeocoding(this IServiceCollection services)
    {
        services.AddHttpClient("Geocoding", client =>
        {
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
        });
        services.AddScoped<IGeocodingService, GeocodingService>();

        return services;
    }
}
