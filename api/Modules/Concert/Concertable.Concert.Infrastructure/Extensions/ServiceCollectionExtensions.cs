using Concertable.Artist.Contracts.Events;
using Concertable.Concert.Contracts;
using Concertable.Concert.Domain.Events;
using Concertable.Concert.Infrastructure.Events;
using Concertable.Concert.Infrastructure.Handlers;
using Concertable.Shared;
using Concertable.Venue.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConcertModule(this IServiceCollection services)
    {
        services.AddScoped<IConcertModule, ConcertModule>();

        services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueReadModelProjectionHandler>();
        return services;
    }
}
