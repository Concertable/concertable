using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Mappings;
using Application.Serializers;
using Core.Entities;
using Core.Entities.Identity;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;
using Web.Authorization;

namespace Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    opt => opt.UseNetTopologySuite()
                ));

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddApiEndpoints();

            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));

            services.AddSingleton<GeometryFactory>(
                NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

            services.AddScoped(provider => new Lazy<IEventService>(() => provider.GetRequiredService<IEventService>()));
            services.AddScoped(provider => new Lazy<ITicketService>(() => provider.GetRequiredService<ITicketService>()));

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueueHostedService>();

            services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
            {
                client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/");
            });

            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSignalR();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IVenueService, VenueService>();
            services.AddScoped<IArtistService, ArtistService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IListingApplicationService, ListingApplicationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IListingService, ListingService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IQrCodeService, QrCodeService>();
            services.AddScoped<IUserPaymentService, UserPaymentService>();
            services.AddScoped<IStripeAccountService, StripeAccountService>();
            services.AddScoped<IPreferenceService, PreferenceService>();
            services.AddScoped<IUriService, UriService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IListingApplicationValidationService, ListingApplicationValidationService>();
            services.AddScoped<ITicketValidationService, TicketValidationService>();
            services.AddScoped<IGeometryProvider, GeometryProvider>();
            services.AddScoped<IStripeValidationService, StripeValidationService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IOwnershipService, OwnershipService>();
            services.AddScoped<IEventValidationService, EventValidationService>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<IArtistRepository, ArtistRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
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

            services.AddScoped<IGeometrySpecification<Event>>(sp =>
                new GeometrySpecification<Event>(
                    sp.GetRequiredService<IGeometryProvider>(),
                    (center, radius) => e => e.Application.Listing.Venue.User.Location != null && e.Application.Listing.Venue.User.Location.Distance(center) <= radius * 1000));

            services.AddScoped<ISearchSpecification<Artist>, SearchSpecification<Artist>>();
            services.AddScoped<ISearchSpecification<Venue>, SearchSpecification<Venue>>();
            services.AddScoped<ISearchSpecification<Event>, SearchSpecification<Event>>();

            services.AddScoped<IArtistSearchSpecification, ArtistSearchSpecification>();
            services.AddScoped<IVenueSearchSpecification, VenueSearchSpecification>();
            services.AddScoped<IEventSearchSpecification, EventSearchSpecification>();

            services.AddScoped<IArtistSearchRepository, ArtistSearchRepository>();
            services.AddScoped<IVenueSearchRepository, VenueSearchRepository>();
            services.AddScoped<IEventSearchRepository, EventSearchRepository>();

            services.AddKeyedScoped<ISearchService, ArtistSearchService>("artist");
            services.AddKeyedScoped<ISearchService, VenueSearchService>("venue");
            services.AddKeyedScoped<ISearchService, EventSearchService>("event");

            services.AddScoped<ISearchServiceFactory, SearchServiceFactory>();

            return services;
        }

        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddAuthentication();
            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.Cookie.SameSite = SameSiteMode.None;
                config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                config.ExpireTimeSpan = TimeSpan.FromDays(7);
                config.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                config.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization()
                .AddSingleton<IAuthorizationHandler, AdminAuthorizeHandler>();

            return services;
        }
    }
}
