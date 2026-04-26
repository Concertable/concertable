using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Concertable.Messaging.Application.Interfaces;
using Concertable.Messaging.Contracts;
using Concertable.Messaging.Infrastructure.Data;
using Concertable.Messaging.Infrastructure.Repositories;
using Concertable.Messaging.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Messaging.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MessagingDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddSingleton<MessagingConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<MessagingConfigurationProvider>());

        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMessagingModule, MessagingModule>();

        return services;
    }
}
