using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories;

public class ContractServiceFactory<T> : IContractServiceFactory<T> where T : class, IContractWorkflow
{
    private readonly IServiceProvider serviceProvider;

    public ContractServiceFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public T Create(ContractType contractType)
        => serviceProvider.GetRequiredKeyedService<T>(contractType);
}
