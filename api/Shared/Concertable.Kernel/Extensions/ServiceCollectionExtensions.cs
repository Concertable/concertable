using Concertable.Shared.Infrastructure.Background;
using Concertable.Shared.Infrastructure.Events;
using Concertable.Shared.Infrastructure.Services;
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

        services.Configure<UrlSettings>(configuration.GetSection("Urls"));
        services.AddScoped<IUriService, UriService>();

        return services;
    }

    public static IServiceCollection AddQueueHostedService(this IServiceCollection services)
    {
        services.AddHostedService<QueueHostedService>();
        return services;
    }

    public static IServiceCollection AddClientCredentials(
        this IServiceCollection services,
        Action<TokenServiceOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<ITokenService, ClientCredentialsTokenService>();
        return services;
    }
}
