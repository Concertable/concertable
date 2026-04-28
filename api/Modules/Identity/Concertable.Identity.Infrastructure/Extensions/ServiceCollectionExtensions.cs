using Concertable.Artist.Contracts.Events;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure.Data;
using Concertable.Identity.Application.Validators;
using Concertable.Identity.Domain.Events;
using Concertable.Identity.Infrastructure.Data;
using Concertable.Identity.Infrastructure.Data.Seeders;
using Concertable.Identity.Infrastructure.Events;
using Concertable.Venue.Contracts.Events;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Identity.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>((sp, opt) =>
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

        services.AddScoped<IAuthUserSeam, AuthUserSeam>();

        services.AddScoped<IdentityModule>();
        services.AddScoped<IManagerModule>(sp => sp.GetRequiredService<IdentityModule>());
        services.AddScoped<IIdentityModule>(sp => sp.GetRequiredService<IdentityModule>());

        services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>, UserCreatedDomainEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistManagerSyncHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueManagerSyncHandler>();

        services.AddSingleton<IdentityConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<IdentityConfigurationProvider>());

        services.AddValidatorsFromAssemblyContaining<UpdateLocationRequestValidator>();

        return services;
    }

    public static IServiceCollection AddIdentityDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, IdentityDevSeeder>();
        return services;
    }

    public static IServiceCollection AddIdentityTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, IdentityTestSeeder>();
        return services;
    }
}
