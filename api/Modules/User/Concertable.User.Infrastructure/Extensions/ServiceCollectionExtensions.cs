using Concertable.Artist.Contracts.Events;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure.Data;
using Concertable.User.Application.Validators;
using Concertable.User.Domain.Events;
using Concertable.User.Infrastructure.Data;
using Concertable.User.Infrastructure.Data.Seeders;
using Concertable.User.Infrastructure.Events;
using Concertable.Venue.Contracts.Events;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.User.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>((sp, opt) =>
            opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOpt => sqlOpt.UseNetTopologySuite())
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IUserMapper, UserMapper>();

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

        services.AddScoped<IUserModule, UserModule>();

        services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>, UserCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistManagerSyncHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueManagerSyncHandler>();

        services.AddSingleton<UserConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<UserConfigurationProvider>());

        services.AddValidatorsFromAssemblyContaining<UpdateLocationRequestValidator>();

        return services;
    }

    public static IServiceCollection AddUserDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, UserDevSeeder>();
        return services;
    }

    public static IServiceCollection AddUserTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, UserTestSeeder>();
        return services;
    }
}
