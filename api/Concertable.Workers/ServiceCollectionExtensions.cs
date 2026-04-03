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
        services.AddScoped<IVenueManagerRepository, VenueManagerRepository>();
        services.AddScoped<IArtistManagerRepository, ArtistManagerRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripeAccountService, StripeAccountService>();

        services.AddScoped(typeof(IContractStrategyFactory<>), typeof(ContractStrategyFactory<>));
        services.AddScoped(typeof(IContractStrategyResolver<>), typeof(ContractStrategyResolver<>));

        services.AddScoped<ICompleteProcessor, CompleteProcessor>();
        services.AddScoped<ISettlementProcessor, SettlementProcessor>();
        services.AddKeyedScoped<IApplicationStrategy, FlatFeeApplicationService>(ContractType.FlatFee);
        services.AddKeyedScoped<IApplicationStrategy, DoorSplitApplicationService>(ContractType.DoorSplit);
        services.AddKeyedScoped<IApplicationStrategy, VersusApplicationService>(ContractType.Versus);
        services.AddKeyedScoped<IApplicationStrategy, VenueHireApplicationService>(ContractType.VenueHire);


        return services;
    }
}
