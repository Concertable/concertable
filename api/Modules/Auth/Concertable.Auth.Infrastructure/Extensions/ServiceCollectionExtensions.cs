using Concertable.Auth.Application.Validators;
using Concertable.Auth.Infrastructure.Data;
using Concertable.Auth.Infrastructure.Data.Seeders;
using Concertable.Auth.Infrastructure.Repositories;
using Concertable.Auth.Infrastructure.Services;
using Concertable.Auth.Infrastructure.Settings;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Concertable.Auth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>((sp, opt) =>
            opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.Configure<AuthSettings>(configuration.GetSection("Auth"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IAuthUriService, AuthUriService>();

        services.AddSingleton<JwtSecurityTokenHandler>();
        services.AddSingleton<RandomNumberGenerator>(_ => RandomNumberGenerator.Create());
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.AddScoped<IAuthModule, AuthModule>();

        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }

    public static IServiceCollection AddAuthDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<Concertable.Application.Interfaces.IDevSeeder, AuthDevSeeder>();
        return services;
    }

    public static IServiceCollection AddAuthTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<Concertable.Application.Interfaces.ITestSeeder, AuthTestSeeder>();
        return services;
    }
}
