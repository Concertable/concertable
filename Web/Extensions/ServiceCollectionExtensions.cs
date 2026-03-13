using Application.Interfaces;
using Core.Enums;
using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Application.Interfaces.Search;
using Application.Serializers;
using Core.Entities;
using Infrastructure;
using Infrastructure.Background;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Search;
using Infrastructure.Services;
using Infrastructure.Services.Search;
using Infrastructure.Settings;
using Infrastructure.Factories;
using Infrastructure.Specifications;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using QuestPDF.Infrastructure;
using Web.Authorization;
using Application.DTOs;

namespace Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));

        if (!string.IsNullOrEmpty(configuration.GetSection("BlobStorage")["ConnectionString"]))
            services.AddScoped<IBlobStorageService, BlobStorageService>();
        else
            services.AddScoped<IBlobStorageService, FakeBlobStorageService>();

        services.AddSingleton<GeometryFactory>(
            NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

        services.AddScoped(provider => new Lazy<IConcertService>(() => provider.GetRequiredService<IConcertService>()));
        services.AddScoped(provider => new Lazy<ITicketService>(() => provider.GetRequiredService<ITicketService>()));

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueueHostedService>();

        services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
        {
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
        });

        services.AddSignalR();

        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IListingApplicationService, ListingApplicationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IManagerService, ManagerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IUserPaymentService, UserPaymentService>();
        services.AddScoped<IStripeAccountService, StripeAccountService>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.AddScoped<IUriService, UriService>();
        services.AddScoped<IListingApplicationValidationService, ListingApplicationValidationService>();
        services.AddScoped<ITicketValidationService, TicketValidationService>();
        services.AddScoped<IGeometryProvider, GeometryProvider>();
        services.AddScoped<IStripeValidationService, StripeValidationService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IOwnershipService, OwnershipService>();
        services.AddScoped<IConcertValidationService, ConcertValidationService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IListingApplicationRepository, ListingApplicationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IStripeEventRepository, StripeEventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        services.AddScoped<IGeometrySpecification<Artist>>(sp =>
            new GeometrySpecification<Artist>(
                sp.GetRequiredService<IGeometryProvider>(),
                (center, radius) => a => a.User.Location != null && a.User.Location.Distance(center) <= radius * 1000));

        services.AddScoped<IGeometrySpecification<Venue>>(sp =>
            new GeometrySpecification<Venue>(
                sp.GetRequiredService<IGeometryProvider>(),
                (center, radius) => v => v.User.Location != null && v.User.Location.Distance(center) <= radius * 1000));

        services.AddScoped<IGeometrySpecification<Concert>>(sp =>
            new GeometrySpecification<Concert>(
                sp.GetRequiredService<IGeometryProvider>(),
                (center, radius) => e => e.Application.Listing.Venue.User.Location != null && e.Application.Listing.Venue.User.Location!.Distance(center) <= radius * 1000));

        services.AddScoped<ISearchSpecification<Artist>, SearchSpecification<Artist>>();
        services.AddScoped<ISearchSpecification<Venue>, SearchSpecification<Venue>>();
        services.AddScoped<ISearchSpecification<Concert>, SearchSpecification<Concert>>();

        services.AddScoped<IArtistSearchSpecification, ArtistSearchSpecification>();
        services.AddScoped<IVenueSearchSpecification, VenueSearchSpecification>();
        services.AddScoped<IConcertSearchSpecification, ConcertSearchSpecification>();

        services.AddScoped<IArtistHeaderRepository, ArtistHeaderRepository>();
        services.AddScoped<IVenueHeaderRepository, VenueHeaderRepository>();
        services.AddScoped<IConcertHeaderRepository, ConcertHeaderRepository>();

        services.AddKeyedScoped<IHeaderService, ArtistHeaderService>(HeaderType.Artist);
        services.AddKeyedScoped<IHeaderService, VenueHeaderService>(HeaderType.Venue);
        services.AddKeyedScoped<IHeaderService, ConcertHeaderService>(HeaderType.Concert);

        services.AddScoped<IHeaderServiceFactory, HeaderServiceFactory>();

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<UserDto>();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        var authSettings = configuration.GetSection("Auth").Get<AuthSettings>()!;
        services.Configure<AuthSettings>(configuration.GetSection("Auth"));

        // Services
        services.AddScoped<IAccountService, AccountService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser>(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return http?.Items[nameof(CurrentUser)] as CurrentUser ?? CurrentUser.Unauthenticated;
        });

        // JWT Bearer
        var keyBytes = Convert.FromBase64String(authSettings.JwtSigningKeyBase64);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Bearer", options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidIssuer = authSettings.Issuer,
                    ValidAudience = authSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization()
            .AddSingleton<IAuthorizationHandler, AdminAuthorizeHandler>();

        return services;
    }
}
