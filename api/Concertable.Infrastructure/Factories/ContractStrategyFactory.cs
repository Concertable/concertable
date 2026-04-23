using Concertable.Application.Interfaces;
using Concertable.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

internal class ContractStrategyFactory<T> : IContractStrategyFactory<T> where T : IContractStrategy
{
    private readonly IServiceProvider serviceProvider;

    public ContractStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public T Create(ContractType contractType)
        => serviceProvider.GetRequiredKeyedService<T>(contractType);
}
