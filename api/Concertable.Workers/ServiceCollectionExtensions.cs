using Concertable.Application.Interfaces.Payment;
using Concertable.Concert.Infrastructure.Extensions;
using Concertable.Contract.Infrastructure.Extensions;
using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Concertable.Identity.Infrastructure.Extensions;
using Infrastructure;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Services.Payment;
using Concertable.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Concertable.Data.Infrastructure;

namespace Workers;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure();
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
        services.AddContractModule();

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripeAccountService, StripeAccountService>();
        services.AddScoped<IManagerPaymentService, ManagerPaymentService>();

        return services;
    }
}
