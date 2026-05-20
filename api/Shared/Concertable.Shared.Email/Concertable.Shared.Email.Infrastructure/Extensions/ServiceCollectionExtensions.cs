using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Email.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var useRealEmail = configuration.GetSection("ExternalServices").GetValue<bool>("UseRealEmail");
        if (useRealEmail)
            services.AddScoped<IEmailService, EmailService>();
        else
            services.AddScoped<IEmailService, FakeEmailService>();

        return services;
    }
}
