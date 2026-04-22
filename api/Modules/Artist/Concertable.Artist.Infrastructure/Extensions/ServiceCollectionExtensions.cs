using Concertable.Artist.Application.Validators;
using Concertable.Artist.Contracts;
using Concertable.Artist.Infrastructure.Data;
using Concertable.Artist.Infrastructure.Data.Seeders;
using Concertable.Artist.Infrastructure.Handlers;
using Concertable.Artist.Infrastructure.Repositories;
using Concertable.Artist.Infrastructure.Services;
using Concertable.Concert.Contracts.Events;
using Concertable.Identity.Contracts.Events;
using Concertable.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Artist.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArtistModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ArtistDbContext>((sp, opt) =>
            opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOpt => sqlOpt.UseNetTopologySuite())
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IArtistModule, ArtistModule>();
        services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, ArtistReviewProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<UserLocationUpdatedEvent>, ArtistLocationSyncHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();

        return services;
    }

    public static IServiceCollection AddArtistDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, ArtistDevSeeder>();
        return services;
    }

    public static IServiceCollection AddArtistTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, ArtistTestSeeder>();
        return services;
    }
}
