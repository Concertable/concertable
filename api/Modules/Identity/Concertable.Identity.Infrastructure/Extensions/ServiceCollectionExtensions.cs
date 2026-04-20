using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Data.Application;
using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Repositories;
using Concertable.Identity.Contracts;
using Concertable.Application.Mappers;
using Concertable.Identity.Infrastructure.Data;
using Concertable.Data.Infrastructure.Data;
using Concertable.Infrastructure.Repositories;
using Concertable.Infrastructure.Services;
using Concertable.Infrastructure.Services.Auth;
using Concertable.Infrastructure.Settings;
using Concertable.Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Concertable.Identity.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>((sp, opt) =>
            opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOpt => sqlOpt.UseNetTopologySuite())
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.Configure<AuthSettings>(configuration.GetSection("Auth"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOwnershipService, OwnershipService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IManagerRepository<ArtistManagerEntity>, ArtistManagerRepository>();
        services.AddScoped<IManagerRepository<VenueManagerEntity>, VenueManagerRepository>();
        services.AddScoped<IUserValidator, UserValidator>();
        services.AddSingleton<IUserMapper, UserMapper>();
        services.AddScoped<IAuthUriService, AuthUriService>();

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
        services.AddScoped<ICurrentUserResolver, CurrentUserResolver>();
        services.AddScoped<IIdentityModule, IdentityModule>();

        return services;
    }
}
