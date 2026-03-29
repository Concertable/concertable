using Concertable.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Blob;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Interfaces.Rating;
using Concertable.Core.Enums;
using Concertable.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Concertable.Application.Interfaces.Search;
using Concertable.Infrastructure.Interfaces;
using Concertable.Application.Serializers;
using Concertable.Core.Entities;
using Infrastructure;
using Concertable.Infrastructure.Background;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Repositories;
using Concertable.Infrastructure.Repositories.Concert;
using Concertable.Infrastructure.Repositories.Rating;
using Concertable.Infrastructure.Repositories.Search;
using Concertable.Infrastructure.Services;
using Concertable.Infrastructure.Services.Auth;
using Concertable.Infrastructure.Services.Blob;
using Concertable.Infrastructure.Services.Concert;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Infrastructure.Services.Accept;
using Concertable.Infrastructure.Services.Application;
using Concertable.Infrastructure.Services.Complete;
using Concertable.Infrastructure.Services.Payment;
using Concertable.Infrastructure.Services.Settlement;
using Concertable.Infrastructure.Services.Webhook;
using Concertable.Infrastructure.Services.Rating;
using Concertable.Infrastructure.Validators;
using Concertable.Infrastructure.Services.Search;
using Concertable.Infrastructure.Settings;
using Concertable.Application.Mappers;
using Concertable.Infrastructure.Factories;
using Concertable.Infrastructure.Mappers;
using Concertable.Infrastructure.Specifications;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using QuestPDF.Infrastructure;
using Concertable.Web.Authorization;
using Concertable.Web.Services;
using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Http;
using QRCoder;
using Concertable.Infrastructure.Data;

namespace Concertable.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOpt => sqlOpt.UseNetTopologySuite())
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));

        if (configuration.GetValue<bool>("UseFakeExternalServices"))
        {
            services.AddScoped<IBlobStorageService, FakeBlobStorageService>();
            services.AddScoped<IStripeAccountService, FakeStripeAccountService>();
            services.AddScoped<IPaymentService, FakePaymentService>();
            services.AddScoped<IWebhookService, FakeWebhookService>();
        }
        else
        {
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<IStripeAccountService, StripeAccountService>();
            services.AddScoped<IStripePaymentClient, StripePaymentClient>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IWebhookService, WebhookService>();
        }

        services.AddKeyedSingleton<IGeometryProvider, GeographicGeometryProvider>(GeometryProviderType.Geographic, (_, _) =>
            new GeographicGeometryProvider(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326)));
        services.AddKeyedSingleton<IGeometryProvider, MetricGeometryProvider>(GeometryProviderType.Metric, (_, _) =>
            new MetricGeometryProvider(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 3857)));


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
        services.AddScoped<IConcertNotificationService, SignalRNotificationService>();
        services.AddScoped<ITicketNotificationService, SignalRNotificationService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IConcertApplicationService, ConcertApplicationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IConcertOpportunityService, ConcertOpportunityService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddSingleton<ITransactionMapperFactory, TransactionMapperFactory>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddSingleton<IGeometryCalculator, GeometryCalculator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddSingleton<QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.AddScoped<IUriService, UriService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IOwnershipService, OwnershipService>();
        services.AddSingleton<ICollectionDiffer, CollectionDiffer>();
        services.AddScoped<IGenreSyncService, GenreSyncService>();
        services.AddContracts();
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
        services.AddScoped<IReviewValidator, ReviewValidator>();

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
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IVenueManagerRepository, VenueManagerRepository>();
        services.AddScoped<IArtistManagerRepository, ArtistManagerRepository>();
        services.AddRatingRepositories();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IStripeEventRepository, StripeEventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDapperRepository, DapperRepository>();

        return services;
    }

    private static IServiceCollection AddRatingRepositories(this IServiceCollection services)
    {
        services.AddKeyedScoped<IRatingRepository, ArtistRatingRepository>(HeaderType.Artist);
        services.AddKeyedScoped<IRatingRepository, ConcertRatingRepository>(HeaderType.Concert);
        services.AddKeyedScoped<IRatingRepository, VenueRatingRepository>(HeaderType.Venue);

        return services;
    }

    private static IServiceCollection AddContracts(this IServiceCollection services)
    {
        services.AddScoped(typeof(IContractStrategyFactory<>), typeof(ContractStrategyFactory<>));
        services.AddScoped(typeof(IContractStrategyResolver<>), typeof(ContractStrategyResolver<>));

        services.AddScoped<IContractService, ContractService>();

        services.AddKeyedSingleton<IContractMapper, FlatFeeContractMapper>(ContractType.FlatFee);
        services.AddKeyedSingleton<IContractMapper, DoorSplitContractMapper>(ContractType.DoorSplit);
        services.AddKeyedSingleton<IContractMapper, VersusContractMapper>(ContractType.Versus);
        services.AddKeyedSingleton<IContractMapper, VenueHireContractMapper>(ContractType.VenueHire);
        services.AddScoped<IContractMapperFactory, ContractMapperFactory>();

        services.AddScoped<ITicketPaymentProcessor, TicketPaymentProcessor>();
        services.AddScoped<IAcceptProcessor, AcceptProcessor>();
        services.AddScoped<ICompleteProcessor, CompleteProcessor>();
        services.AddScoped<ISettlementProcessor, SettlementProcessor>();

        services.AddKeyedScoped<ITicketPaymentStrategy, VenueTicketPaymentService>(ContractType.FlatFee);
        services.AddKeyedScoped<ITicketPaymentStrategy, VenueTicketPaymentService>(ContractType.DoorSplit);
        services.AddKeyedScoped<ITicketPaymentStrategy, VenueTicketPaymentService>(ContractType.Versus);
        services.AddKeyedScoped<ITicketPaymentStrategy, ArtistTicketPaymentService>(ContractType.VenueHire);

        services.AddKeyedScoped<IApplicationStrategy, FlatFeeApplicationService>(ContractType.FlatFee);
        services.AddKeyedScoped<IApplicationStrategy, DoorSplitApplicationService>(ContractType.DoorSplit);
        services.AddKeyedScoped<IApplicationStrategy, VersusApplicationService>(ContractType.Versus);
        services.AddKeyedScoped<IApplicationStrategy, VenueHireApplicationService>(ContractType.VenueHire);

        services.AddScoped<IWebhookStrategyFactory, WebhookStrategyFactory>();
        services.AddScoped<IWebhookProcessor, WebhookProcessor>();
        services.AddScoped<IWebhookQueue, WebhookQueue>();
        services.AddKeyedScoped<IWebhookStrategy, TicketWebhookHandler>(WebhookType.Concert);
        services.AddKeyedScoped<IWebhookStrategy, SettlementWebhookHandler>(WebhookType.Settlement);

        return services;
    }

    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        services.AddSingleton<ILocationSelector<ArtistEntity>, ArtistLocationSelector>();
        services.AddSingleton<ILocationSelector<VenueEntity>, VenueLocationSelector>();
        services.AddSingleton<ILocationSelector<ConcertEntity>, ConcertLocationSelector>();

        services.AddScoped<IGeometrySpecification<ArtistEntity>, GeometrySpecification<ArtistEntity>>();
        services.AddScoped<IGeometrySpecification<VenueEntity>, GeometrySpecification<VenueEntity>>();
        services.AddScoped<IGeometrySpecification<ConcertEntity>, GeometrySpecification<ConcertEntity>>();

        services.AddSingleton<IReviewKeySelector<ArtistEntity>, ArtistReviewKeySelector>();
        services.AddSingleton<IReviewKeySelector<VenueEntity>, VenueReviewKeySelector>();
        services.AddSingleton<IReviewKeySelector<ConcertEntity>, ConcertReviewKeySelector>();

        services.AddScoped<IRatingSpecification<ArtistEntity>, RatingSpecification<ArtistEntity>>();
        services.AddScoped<IRatingSpecification<VenueEntity>, RatingSpecification<VenueEntity>>();
        services.AddScoped<IRatingSpecification<ConcertEntity>, RatingSpecification<ConcertEntity>>();

        services.AddScoped<ISearchSpecification<ArtistEntity>, SearchSpecification<ArtistEntity>>();
        services.AddScoped<ISearchSpecification<VenueEntity>, SearchSpecification<VenueEntity>>();
        services.AddScoped<ISearchSpecification<ConcertEntity>, SearchSpecification<ConcertEntity>>();

        services.AddScoped<IArtistSearchSpecification, ArtistSearchSpecification>();
        services.AddScoped<IVenueSearchSpecification, VenueSearchSpecification>();
        services.AddScoped<IConcertSearchSpecification, ConcertSearchSpecification>();

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
        services.AddValidatorsFromAssemblyContaining<LoginRequest>();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        var authSettings = configuration.GetSection("Auth").Get<AuthSettings>()!;
        services.Configure<AuthSettings>(configuration.GetSection("Auth"));

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<JwtSecurityTokenHandler>();
        services.AddSingleton<RandomNumberGenerator>(_ => RandomNumberGenerator.Create());
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserAccessor>();

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
