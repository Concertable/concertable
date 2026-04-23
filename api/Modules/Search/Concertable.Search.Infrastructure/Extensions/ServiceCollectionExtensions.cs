using Concertable.Core.Enums;
using Concertable.Search.Application;
using Concertable.Search.Domain.Models;
using Concertable.Search.Application.Validators;
using Concertable.Search.Application.Interfaces;
using Concertable.Search.Infrastructure.Data;
using Concertable.Search.Infrastructure.Repositories;
using Concertable.Search.Application.Services;
using Concertable.Search.Infrastructure.Specifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Search.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SearchDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));
        services.AddSingleton<IGeometrySpecification<ArtistSearchModel>, GeometrySpecification<ArtistSearchModel>>();
        services.AddSingleton<IGeometrySpecification<VenueSearchModel>, GeometrySpecification<VenueSearchModel>>();
        services.AddSingleton<IGeometrySpecification<ConcertEntity>, GeometrySpecification<ConcertEntity>>();

        services.AddSingleton<IRatingSpecification<ArtistSearchModel>, ArtistRatingSpecification>();
        services.AddSingleton<IRatingSpecification<VenueSearchModel>, VenueRatingSpecification>();
        services.AddSingleton<IRatingSpecification<ConcertEntity>, ConcertRatingSpecification>();

        services.AddSingleton<ISearchSpecification<ArtistSearchModel>, SearchSpecification<ArtistSearchModel>>();
        services.AddSingleton<ISearchSpecification<VenueSearchModel>, SearchSpecification<VenueSearchModel>>();
        services.AddSingleton<ISearchSpecification<ConcertEntity>, SearchSpecification<ConcertEntity>>();

        services.AddSingleton<IArtistSearchSpecification, ArtistSearchSpecification>();
        services.AddSingleton<IVenueSearchSpecification, VenueSearchSpecification>();
        services.AddSingleton<IConcertSearchSpecification, ConcertSearchSpecification>();

        services.AddSingleton<ISortSpecification<ArtistHeaderDto>, HeaderSortSpecification<ArtistHeaderDto>>();
        services.AddSingleton<ISortSpecification<VenueHeaderDto>, HeaderSortSpecification<VenueHeaderDto>>();
        services.AddSingleton<ISortSpecification<ConcertHeaderDto>, ConcertSortSpecification>();

        services.AddScoped<IHeaderAutocompleteRepository, HeaderAutocompleteRepository>();
        services.AddScoped<IHeaderAutocompleteService, HeaderAutocompleteService>();

        services.AddScoped<IArtistHeaderRepository, ArtistHeaderRepository>();
        services.AddScoped<IVenueHeaderRepository, VenueHeaderRepository>();
        services.AddScoped<IConcertHeaderRepository, ConcertHeaderRepository>();

        services.AddKeyedScoped<IHeaderService, ArtistHeaderService>(HeaderType.Artist);
        services.AddKeyedScoped<IHeaderService, VenueHeaderService>(HeaderType.Venue);
        services.AddKeyedScoped<IHeaderService, ConcertHeaderService>(HeaderType.Concert);
        services.AddScoped<IConcertHeaderModule, ConcertHeaderService>();

        services.AddScoped<IHeaderServiceFactory, HeaderServiceFactory>();

        services.AddScoped<IHeaderModule, SearchModule>();
        services.AddScoped<IAutocompleteModule, AutocompleteModule>();

        services.AddValidatorsFromAssemblyContaining<SearchParamsValidator>();

        return services;
    }
}
