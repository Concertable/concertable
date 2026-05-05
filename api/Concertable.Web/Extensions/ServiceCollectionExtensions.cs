using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Application.Serializers;
using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Repositories;
using Concertable.Shared.Infrastructure.Services.Geometry;
using Concertable.Web.Authorization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
using NetTopologySuite.Geometries;
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

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();
        services.AddSharedDbContext(configuration);

        services.AddReadDbContext(configuration);

        services.AddScoped<IDbConnection>(_ =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IGeometryCalculator, GeometryCalculator>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDapperRepository, DapperRepository>();

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.Authority = configuration["Auth:Authority"];
                options.Audience = "concertable.api";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.Zero
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
