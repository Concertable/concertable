using Concertable.Concert.Infrastructure.Extensions;
using Concertable.Contract.Infrastructure.Extensions;
using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Concertable.Identity.Infrastructure.Extensions;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Notification.Infrastructure.Extensions;
using Concertable.Payment.Infrastructure.Extensions;
using Infrastructure;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Concertable.Data.Infrastructure;

namespace Workers;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure(configuration);
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddSharedDbContext(configuration);
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));

        services.AddReadDbContext(configuration);

        services.AddIdentityModule(configuration);
        services.AddConcertModule(configuration);
        services.AddContractModule(configuration);
        services.AddPaymentModule(configuration);
        services.AddNotificationModule();
        services.AddMessagingModule(configuration);

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
