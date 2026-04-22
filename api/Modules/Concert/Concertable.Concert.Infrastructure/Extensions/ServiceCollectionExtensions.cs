using Concertable.Concert.Domain.Events;
using Concertable.Concert.Infrastructure.Events;
using Concertable.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConcertModule(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();
        return services;
    }
}
