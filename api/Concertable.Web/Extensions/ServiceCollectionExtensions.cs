using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Blob;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Serializers;
using Concertable.Infrastructure.Data;
using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Concertable.Infrastructure.Repositories;
using Concertable.Infrastructure.Services;
using Concertable.Infrastructure.Handlers;
using Concertable.Identity.Contracts.Events;
using Concertable.Infrastructure.Services.Blob;
using Concertable.Infrastructure.Services.Email;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Infrastructure.Settings;
using Concertable.Infrastructure.Validators;
using Concertable.Core.Enums;
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
        services.AddSignalR();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();
        services.AddSharedDbContext(configuration);
        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOpt => sqlOpt.UseNetTopologySuite())
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddReadDbContext(configuration);

        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));

        var external = configuration.GetSection("ExternalServices");

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

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IConcertNotificationService, SignalRConcertNotificationService>();
        services.AddScoped<IApplicationNotificationService, SignalRApplicationNotificationService>();
        services.AddScoped<ITicketNotificationService, SignalRTicketNotificationService>();
        services.AddScoped<IMessageNotificationService, SignalRMessageNotificationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddSingleton<IGeometryCalculator, GeometryCalculator>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddSingleton<QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.Configure<UrlSettings>(configuration.GetSection("Urls"));
        services.AddScoped<IUriService, UriService>();
        services.AddServiceValidators();

        return services;
    }

    public static IServiceCollection AddServiceValidators(this IServiceCollection services)
    {
        services.AddSingleton<IConcertValidator, ConcertValidator>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<IOpportunityApplicationValidator, OpportunityApplicationValidator>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDapperRepository, DapperRepository>();

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
