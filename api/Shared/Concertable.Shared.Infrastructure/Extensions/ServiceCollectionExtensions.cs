using Concertable.Shared.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventBus, InProcessIntegrationEventBus>();
        return services;
    }
}
