using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Enums;
using Concertable.Data.Application;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Concertable.Data.Infrastructure.Events;
using Concertable.Shared;
using Concertable.Identity.Infrastructure.Extensions;
using Infrastructure;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Factories;
using Concertable.Infrastructure.Repositories;
using Concertable.Infrastructure.Repositories.Concert;
using Concertable.Infrastructure.Services.Accept;
using Concertable.Infrastructure.Services.Application;
using Concertable.Infrastructure.Services.Complete;
using Concertable.Infrastructure.Services.Settlement;
using Concertable.Infrastructure.Services.Payment;
using Concertable.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Workers;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));
        services.AddScoped<IReadDbContext, ReadDbContext>();

        services.AddIdentityModule(configuration);

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IOpportunityApplicationRepository, OpportunityApplicationRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripeAccountService, StripeAccountService>();
        services.AddScoped<IManagerPaymentService, ManagerPaymentService>();

        services.AddScoped(typeof(IContractStrategyFactory<>), typeof(ContractStrategyFactory<>));
        services.AddScoped(typeof(IContractStrategyResolver<>), typeof(ContractStrategyResolver<>));

        services.AddScoped<IFinishedDispatcher, FinishedDispatcher>();
        services.AddScoped<ISettlementDispatcher, SettlementDispatcher>();
        services.AddKeyedScoped<IConcertWorkflowStrategy, FlatFeeConcertWorkflow>(ContractType.FlatFee);
        services.AddKeyedScoped<IConcertWorkflowStrategy, DoorSplitConcertWorkflow>(ContractType.DoorSplit);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VersusConcertWorkflow>(ContractType.Versus);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VenueHireConcertWorkflow>(ContractType.VenueHire);

        return services;
    }
}
