using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Core.Enums;
using Infrastructure;
using Infrastructure.Data.Identity;
using Infrastructure.Factories;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Concert;
using Infrastructure.Services.Complete;
using Infrastructure.Services.Payment;
using Infrastructure.Services.Settlement;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IConcertApplicationRepository, ConcertApplicationRepository>();
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
        services.AddKeyedScoped<ICompleteStrategy, FlatFeeCompleteService>(ContractType.FlatFee);
        services.AddKeyedScoped<ICompleteStrategy, DoorSplitCompleteService>(ContractType.DoorSplit);
        services.AddKeyedScoped<ICompleteStrategy, VersusCompleteService>(ContractType.Versus);
        services.AddKeyedScoped<ICompleteStrategy, VenueHireCompleteService>(ContractType.VenueHire);

        services.AddScoped<ISettlementProcessor, SettlementProcessor>();
        services.AddKeyedScoped<ISettlementStrategy, FlatFeeSettlementService>(ContractType.FlatFee);
        services.AddKeyedScoped<ISettlementStrategy, DoorSplitSettlementService>(ContractType.DoorSplit);
        services.AddKeyedScoped<ISettlementStrategy, VersusSettlementService>(ContractType.Versus);
        services.AddKeyedScoped<ISettlementStrategy, VenueHireSettlementService>(ContractType.VenueHire);

        return services;
    }
}
