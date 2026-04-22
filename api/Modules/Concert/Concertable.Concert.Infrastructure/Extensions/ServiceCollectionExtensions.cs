using Concertable.Application.Interfaces;
using Concertable.Artist.Contracts.Events;
using Concertable.Concert.Contracts;
using Concertable.Concert.Domain.Events;
using Concertable.Concert.Infrastructure.Data.Seeders;
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

    public static IServiceCollection AddConcertDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, ConcertDevSeeder>();
        return services;
    }

    public static IServiceCollection AddConcertTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, ConcertTestSeeder>();
        return services;
    }
}
