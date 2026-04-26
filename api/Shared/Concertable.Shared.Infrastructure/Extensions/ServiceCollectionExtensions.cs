using Concertable.Shared.Infrastructure.Background;
using Concertable.Shared.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventBus, InProcessIntegrationEventBus>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IBackgroundTaskRunner, BackgroundTaskRunner>();
        return services;
    }

    public static IServiceCollection AddQueueHostedService(this IServiceCollection services)
    {
        services.AddHostedService<QueueHostedService>();
        return services;
    }
}
