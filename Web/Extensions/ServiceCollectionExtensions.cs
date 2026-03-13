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
using Infrastructure.Validators;
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
using Application.Requests;
using Core.Parameters;
using Microsoft.AspNetCore.Http;
using QRCoder;

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


        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueueHostedService>();

        services.AddHttpClient("Geocoding", client =>
        {
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
        });
        services.AddScoped<IGeocodingService, GeocodingService>();

        services.AddSignalR();

        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IConcertApplicationService, ConcertApplicationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IConcertOpportunityService, ConcertOpportunityService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddSingleton<IGeometryCalculator, GeometryCalculator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddSingleton<QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IUserPaymentService, UserPaymentService>();
        services.AddScoped<IStripeAccountService, StripeAccountService>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.AddScoped<IUriService, UriService>();
        services.AddSingleton<IGeometryProvider, GeometryProvider>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IOwnershipService, OwnershipService>();
        services.AddValidators();

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddSingleton<IConcertValidator, ConcertValidator>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<IConcertApplicationValidator, ConcertApplicationValidator>();
        services.AddScoped<IStripeValidator, StripeValidator>();
        services.AddScoped<IUserValidator, UserValidator>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IConcertApplicationRepository, ConcertApplicationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConcertOpportunityRepository, ConcertOpportunityRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddRatingRepositories();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IStripeEventRepository, StripeEventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddRatingRepositories(this IServiceCollection services)
    {
        services.AddKeyedScoped<IRatingRepository, ArtistRatingRepository>(HeaderType.Artist);
        services.AddKeyedScoped<IRatingRepository, ConcertRatingRepository>(HeaderType.Concert);
        services.AddKeyedScoped<IRatingRepository, VenueRatingRepository>(HeaderType.Venue);

        return services;
    }

    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        services.AddSingleton<ILocationSelector<Artist>, ArtistLocationSelector>();
        services.AddSingleton<ILocationSelector<Venue>, VenueLocationSelector>();
        services.AddSingleton<ILocationSelector<Concert>, ConcertLocationSelector>();

        services.AddScoped<IGeometrySpecification<Artist>, GeometrySpecification<Artist>>();
        services.AddScoped<IGeometrySpecification<Venue>, GeometrySpecification<Venue>>();
        services.AddScoped<IGeometrySpecification<Concert>, GeometrySpecification<Concert>>();

        services.AddSingleton<IReviewKeySelector<Artist>, ArtistReviewKeySelector>();
        services.AddSingleton<IReviewKeySelector<Venue>, VenueReviewKeySelector>();
        services.AddSingleton<IReviewKeySelector<Concert>, ConcertReviewKeySelector>();

        services.AddScoped<IRatingSpecification<Artist>, RatingSpecification<Artist>>();
        services.AddScoped<IRatingSpecification<Venue>, RatingSpecification<Venue>>();
        services.AddScoped<IRatingSpecification<Concert>, RatingSpecification<Concert>>();

        services.AddScoped<ISearchSpecification<Artist>, SearchSpecification<Artist>>();
        services.AddScoped<ISearchSpecification<Venue>, SearchSpecification<Venue>>();
        services.AddScoped<ISearchSpecification<Concert>, SearchSpecification<Concert>>();

        services.AddScoped<IArtistSearchSpecification, ArtistSearchSpecification>();
        services.AddScoped<IVenueSearchSpecification, VenueSearchSpecification>();
        services.AddScoped<IConcertSearchSpecification, ConcertSearchSpecification>();

        services.AddScoped<ISearchRatingSpecification<Artist>, SearchRatingSpecification<Artist>>();
        services.AddScoped<ISearchRatingSpecification<Venue>, SearchRatingSpecification<Venue>>();
        services.AddScoped<ISearchRatingSpecification<Concert>, SearchRatingSpecification<Concert>>();

        services.AddScoped<IArtistHeaderSpecification, ArtistHeaderSpecification>();
        services.AddScoped<IVenueHeaderSpecification, VenueHeaderSpecification>();
        services.AddScoped<IConcertHeaderSpecification, ConcertHeaderSpecification>();

        services.AddScoped<IArtistHeaderRepository, ArtistHeaderRepository>();
        services.AddScoped<IVenueHeaderRepository, VenueHeaderRepository>();
        services.AddScoped<IConcertHeaderRepository, ConcertHeaderRepository>();

        services.AddKeyedScoped<IHeaderService, ArtistHeaderService>(HeaderType.Artist);
        services.AddKeyedScoped<IHeaderService, VenueHeaderService>(HeaderType.Venue);
        services.AddKeyedScoped<IHeaderService, ConcertHeaderService>(HeaderType.Concert);
        services.AddScoped<IConcertHeaderService, ConcertHeaderService>();

        services.AddScoped<IHeaderServiceFactory, HeaderServiceFactory>();

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
        services.AddScoped<IValidator<ChangeEmailRequest>, ChangeEmailRequestValidator>();
        services.AddScoped<IValidator<ConcertOpportunityDto>, ConcertOpportunityDtoValidator>();
        services.AddScoped<IValidator<MarkMessagesReadRequest>, MarkMessagesReadRequestValidator>();
        services.AddScoped<IValidator<CreatePreferenceRequest>, CreatePreferenceRequestValidator>();
        services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewRequestValidator>();
        services.AddScoped<IValidator<TicketPurchaseParams>, TicketPurchaseParamsValidator>();
        services.AddScoped<IValidator<UpdateLocationRequest>, UpdateLocationRequestValidator>();
        services.AddScoped<IValidator<CreateArtistRequest>, CreateArtistRequestValidator>();
        services.AddScoped<IValidator<UpdateArtistRequest>, UpdateArtistRequestValidator>();
        services.AddScoped<IValidator<UpdateConcertRequest>, UpdateConcertRequestValidator>();
        services.AddScoped<IValidator<IFormFile>, IFormFileValidator>();
        services.AddScoped<IValidator<CreateVenueRequest>, CreateVenueRequestValidator>();
        services.AddScoped<IValidator<UpdateVenueRequest>, UpdateVenueRequestValidator>();

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
