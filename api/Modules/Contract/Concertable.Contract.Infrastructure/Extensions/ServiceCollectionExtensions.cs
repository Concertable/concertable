using Concertable.Contract.Application.Interfaces;
using Concertable.Contract.Application.Mappers;
using Concertable.Contract.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Contract.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContractModule(this IServiceCollection services)
    {
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IContractMapper, ContractMapper>();
        services.AddScoped<IContractModule, ContractModule>();
        return services;
    }
}
