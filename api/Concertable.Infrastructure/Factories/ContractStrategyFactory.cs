using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

public class ContractStrategyFactory<T> : IContractStrategyFactory<T> where T : IContractStrategy
{
    private readonly IServiceProvider serviceProvider;

    public ContractStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public T Create(ContractType contractType)
        => serviceProvider.GetRequiredKeyedService<T>(contractType);
}
