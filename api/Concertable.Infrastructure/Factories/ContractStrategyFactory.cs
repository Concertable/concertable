using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories;

public class ContractStrategyFactory<T> : IContractStrategyFactory<T> where T : IContractStrategy
{
    private readonly IServiceProvider serviceProvider;

    public ContractStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public T Create(ContractType contractType)
        => serviceProvider.GetRequiredKeyedService<T>(contractType);
}
