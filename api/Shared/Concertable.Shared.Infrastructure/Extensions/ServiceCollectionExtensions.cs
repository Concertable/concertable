using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Blob;
using Concertable.Shared.Infrastructure.Background;
using Concertable.Shared.Infrastructure.Events;
using Concertable.Shared.Infrastructure.Services;
using Concertable.Shared.Infrastructure.Services.Blob;
using Concertable.Shared.Infrastructure.Services.Email;
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
        services.AddScoped<IIntegrationEventBus, InProcessIntegrationEventBus>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IBackgroundTaskRunner, BackgroundTaskRunner>();

        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));

        var external = configuration.GetSection("ExternalServices");

        if (external.GetValue<bool>("UseRealBlob"))
            services.AddScoped<IBlobStorageService, BlobStorageService>();
        else
            services.AddScoped<IBlobStorageService, FakeBlobStorageService>();

        if (external.GetValue<bool>("UseRealEmail"))
            services.AddScoped<IEmailService, EmailService>();
        else
            services.AddScoped<IEmailService, FakeEmailService>();

        if (external.GetValue<bool>("UseRealImages"))
            services.AddScoped<IImageService, ImageService>();
        else
            services.AddScoped<IImageService, FakeImageService>();

        services.AddHttpClient("Geocoding", client =>
        {
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
        });
        services.AddScoped<IGeocodingService, GeocodingService>();

        services.Configure<UrlSettings>(configuration.GetSection("Urls"));
        services.AddScoped<IUriService, UriService>();
        services.AddScoped<IGenreService, GenreService>();

        return services;
    }

    public static IServiceCollection AddQueueHostedService(this IServiceCollection services)
    {
        services.AddHostedService<QueueHostedService>();
        return services;
    }
}
