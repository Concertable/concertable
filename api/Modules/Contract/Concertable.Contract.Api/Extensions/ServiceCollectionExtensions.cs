using Concertable.Contract.Api.Controllers;
using Concertable.Contract.Application.Interfaces;
using Concertable.Contract.Application.Services;
using Concertable.Contract.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Contract.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContractApi(this IServiceCollection services)
    {
        services.AddContractModule();
        services.AddScoped<IContractService, ContractService>();
        services.AddControllers()
            .AddApplicationPart(typeof(ContractController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
