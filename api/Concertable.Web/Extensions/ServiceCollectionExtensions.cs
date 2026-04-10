using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Blob;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Interfaces.Rating;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Serializers;
using Concertable.Application.Validators;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Background;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Expressions.Selectors;
using Concertable.Infrastructure.Factories;
using Concertable.Infrastructure.Interfaces;
using Concertable.Infrastructure.Mappers;
using Concertable.Infrastructure.Repositories;
using Concertable.Infrastructure.Repositories.Concert;
using Concertable.Infrastructure.Repositories.Rating;
using Concertable.Infrastructure.Repositories.Search;
using Concertable.Infrastructure.Services;
using Concertable.Infrastructure.Services.Accept;
using Concertable.Infrastructure.Services.Application;
using Concertable.Infrastructure.Services.Auth;
using Concertable.Infrastructure.Services.Blob;
using Concertable.Infrastructure.Services.Complete;
using Concertable.Infrastructure.Services.Concert;
using Concertable.Infrastructure.Services.Email;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Infrastructure.Services.Payment;
using Concertable.Infrastructure.Services.Rating;
using Concertable.Infrastructure.Services.Search;
using Concertable.Infrastructure.Services.Settlement;
using Concertable.Infrastructure.Services.Webhook;
using Concertable.Infrastructure.Settings;
using Concertable.Infrastructure.Specifications;
using Concertable.Infrastructure.Validators;
using Concertable.Web.Authorization;
using Concertable.Web.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using QRCoder;
using QuestPDF.Infrastructure;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Concertable.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddSingleton(TimeProvider.System);

        services.AddKeyedSingleton<IGeometryProvider, GeographicGeometryProvider>(GeometryProviderType.Geographic, (_, _) =>
            new GeographicGeometryProvider(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326)));
        services.AddKeyedSingleton<IGeometryProvider, MetricGeometryProvider>(GeometryProviderType.Metric, (_, _) =>
            new MetricGeometryProvider(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 3857)));

        services.AddDatabase(configuration);
        services.AddExternalServices(configuration);
        services.AddBackgroundServices();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOpt => sqlOpt.UseNetTopologySuite())
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    private static IServiceCollection AddStripeServices(this IServiceCollection services, bool useRealStripe)
    {
        services.AddScoped<StripeAccountValidator>();
        services.AddScoped<StripeCustomerValidator>();
        services.AddScoped<IStripeValidator, StripeValidator>();
        services.AddScoped<IStripeValidationFactory, StripeValidationFactory>();
        services.AddKeyedScoped<IStripeValidationStrategy, StripeAccountValidator>(ContractType.VenueHire);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.FlatFee);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.DoorSplit);
        services.AddKeyedScoped<IStripeValidationStrategy, StripeCustomerValidator>(ContractType.Versus);

        if (useRealStripe)
        {
            services.AddSingleton<Stripe.AccountService>();
            services.AddSingleton<Stripe.AccountLinkService>();
            services.AddSingleton<Stripe.CustomerService>();
            services.AddSingleton<Stripe.PaymentMethodService>();
            services.AddSingleton<Stripe.SetupIntentService>();
            services.AddScoped<IStripeAccountService, StripeAccountService>();
            services.AddSingleton<IStripePaymentClient, StripePaymentClient>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IWebhookService, WebhookService>();
        }
        else
        {
            services.AddScoped<IStripeAccountService, FakeStripeAccountService>();
            services.AddScoped<IPaymentService, FakePaymentService>();
            services.AddScoped<IWebhookService, FakeWebhookService>();
        }

        return services;
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));

        var external = configuration.GetSection("ExternalServices");

        services.AddStripeServices(external.GetValue<bool>("UseRealStripe"));

        if (external.GetValue<bool>("UseRealBlob"))
            services.AddScoped<IBlobStorageService, BlobStorageService>();
        else
            services.AddScoped<IBlobStorageService, FakeBlobStorageService>();

        if (external.GetValue<bool>("UseRealEmail"))
            services.AddScoped<IEmailService, EmailService>();
        else
            services.AddScoped<IEmailService, FakeEmailService>();

        if (external.GetValue<bool>("UseRealImages"))
            services.AddScoped<IImageService, ImageService>();
        else
            services.AddScoped<IImageService, FakeImageService>();

        services.AddHttpClient("Geocoding", client =>
        {
            client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
        });
        services.AddScoped<IGeocodingService, GeocodingService>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueueHostedService>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IConcertNotificationService, SignalRConcertNotificationService>();
        services.AddScoped<IApplicationNotificationService, SignalRApplicationNotificationService>();
        services.AddScoped<ITicketNotificationService, SignalRTicketNotificationService>();
        services.AddScoped<IMessageNotificationService, SignalRMessageNotificationService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IOpportunityApplicationService, OpportunityApplicationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddSingleton<ITransactionMapper, TransactionMapper>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddSingleton<IGeometryCalculator, GeometryCalculator>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddSingleton<QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.Configure<UrlSettings>(configuration.GetSection("Urls"));
        services.AddScoped<IUriService, UriService>();
        services.AddScoped<IAuthUriService, AuthUriService>();
        services.AddScoped<IOwnershipService, OwnershipService>();
        services.AddSingleton<ICollectionDiffer, CollectionDiffer>();
        services.AddScoped<IGenreSyncService, GenreSyncService>();
        services.AddContracts();
        services.AddServiceValidators();

        return services;
    }

    public static IServiceCollection AddServiceValidators(this IServiceCollection services)
    {
        services.AddSingleton<IConcertValidator, ConcertValidator>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<IOpportunityApplicationValidator, OpportunityApplicationValidator>();
        services.AddScoped<IUserValidator, UserValidator>();
        services.AddScoped<IReviewValidator, ReviewValidator>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IOpportunityApplicationRepository, OpportunityApplicationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
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

        services.AddSingleton<IContractMapper, ContractMapper>();
        services.AddSingleton<IContractServiceStrategy, ContractServiceStrategy>();
        services.AddSingleton<IUserMapper, UserMapper>();
        services.AddSingleton<IOpportunityMapper, OpportunityMapper>();
        services.AddSingleton<IOpportunityApplicationMapper, OpportunityApplicationMapper>();

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
        services.AddSingleton<IGeometrySpecification<ArtistEntity>, GeometrySpecification<ArtistEntity>>();
        services.AddSingleton<IGeometrySpecification<VenueEntity>, GeometrySpecification<VenueEntity>>();
        services.AddSingleton<IGeometrySpecification<ConcertEntity>, GeometrySpecification<ConcertEntity>>();

        services.AddSingleton<IReviewKeySelector<ArtistEntity>, ArtistReviewKeySelector>();
        services.AddSingleton<IReviewKeySelector<VenueEntity>, VenueReviewKeySelector>();
        services.AddSingleton<IReviewKeySelector<ConcertEntity>, ConcertReviewKeySelector>();

        services.AddSingleton<IRatingSpecification<ArtistEntity>, RatingSpecification<ArtistEntity>>();
        services.AddSingleton<IRatingSpecification<VenueEntity>, RatingSpecification<VenueEntity>>();
        services.AddSingleton<IRatingSpecification<ConcertEntity>, RatingSpecification<ConcertEntity>>();

        services.AddSingleton<ISearchSpecification<ArtistEntity>, SearchSpecification<ArtistEntity>>();
        services.AddSingleton<ISearchSpecification<VenueEntity>, SearchSpecification<VenueEntity>>();
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
        var authSettings = configuration.GetSection("Auth").Get<AuthSettings>()!;
        services.Configure<AuthSettings>(configuration.GetSection("Auth"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserLoader, UserLoader>();
        services.AddKeyedScoped<IUserLoader, VenueManagerLoader>(Role.VenueManager);
        services.AddKeyedScoped<IUserLoader, ArtistManagerLoader>(Role.ArtistManager);
        services.AddKeyedScoped<IUserLoader, CustomerLoader>(Role.Customer);
        services.AddKeyedScoped<IUserLoader, AdminLoader>(Role.Admin);

        services.AddScoped<IUserRegister, UserRegister>();
        services.AddKeyedScoped<IUserRegister, VenueManagerRegister>(Role.VenueManager);
        services.AddKeyedScoped<IUserRegister, ArtistManagerRegister>(Role.ArtistManager);
        services.AddKeyedScoped<IUserRegister, CustomerRegister>(Role.Customer);
        services.AddKeyedScoped<IUserRegister, AdminRegister>(Role.Admin);
        services.AddSingleton<JwtSecurityTokenHandler>();
        services.AddSingleton<RandomNumberGenerator>(_ => RandomNumberGenerator.Create());
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserAccessor>();

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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization()
            .AddSingleton<IAuthorizationHandler, AdminAuthorizeHandler>();

        return services;
    }
}
