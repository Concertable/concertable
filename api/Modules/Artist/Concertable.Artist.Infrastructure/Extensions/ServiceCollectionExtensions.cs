using Concertable.Artist.Application.Validators;
using Concertable.Artist.Infrastructure.Data;
using Concertable.Artist.Infrastructure.Handlers;
using Concertable.Artist.Infrastructure.Repositories;
using Concertable.Artist.Infrastructure.Services;
using Concertable.Concert.Contracts.Events;
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
        services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, ArtistReviewProjectionHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();

        return services;
    }
}
