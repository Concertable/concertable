using Concertable.Application.Interfaces;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Data.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReadDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));
        services.AddScoped<IReadDbContext, ReadDbContext>();
        return services;
    }

    public static IServiceCollection AddSharedDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SharedDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddSharedDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, SharedDevSeeder>();
        return services;
    }

    public static IServiceCollection AddSharedTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, SharedTestSeeder>();
        return services;
    }
}
