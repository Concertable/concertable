using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Enums;

namespace Infrastructure.Factories;

public class ContractMapperFactory : IContractMapperFactory
{
    private readonly IContractServiceFactory<IContractMapper> factory;

    public ContractMapperFactory(IContractServiceFactory<IContractMapper> factory)
    {
        this.factory = factory;
    }

    public IContractMapper Create(ContractType type) =>
        factory.Create(type);
}
