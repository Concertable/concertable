using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Enums;
using Infrastructure;
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
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;

namespace Workers;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite()));

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));

        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IConcertRepository, ConcertRepository>();
        services.AddScoped<IOpportunityApplicationRepository, OpportunityApplicationRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IManagerRepository<VenueManagerEntity>, VenueManagerRepository>();
        services.AddScoped<IManagerRepository<ArtistManagerEntity>, ArtistManagerRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripeAccountService, StripeAccountService>();
        services.AddScoped<IManagerPaymentService, ManagerPaymentService>();

        services.AddScoped(typeof(IContractStrategyFactory<>), typeof(ContractStrategyFactory<>));
        services.AddScoped(typeof(IContractStrategyResolver<>), typeof(ContractStrategyResolver<>));

        services.AddScoped<IFinishedProcessor, FinishedProcessor>();
        services.AddScoped<ISettlementProcessor, SettlementProcessor>();
        services.AddKeyedScoped<IConcertWorkflowStrategy, FlatFeeConcertWorkflow>(ContractType.FlatFee);
        services.AddKeyedScoped<IConcertWorkflowStrategy, DoorSplitConcertWorkflow>(ContractType.DoorSplit);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VersusConcertWorkflow>(ContractType.Versus);
        services.AddKeyedScoped<IConcertWorkflowStrategy, VenueHireConcertWorkflow>(ContractType.VenueHire);


        return services;
    }
}
