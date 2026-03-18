using Application.Interfaces.Concert;
using Core.Enums;

namespace Infrastructure.Factories;

public class ContractMapperFactory : IContractMapperFactory
{
    private readonly IDictionary<ContractType, IContractMapper> mappers;

    public ContractMapperFactory(IEnumerable<IContractMapper> mappers)
    {
        this.mappers = mappers.ToDictionary(m => m.ContractType);
    }

    public IContractMapper Create(ContractType type) => mappers[type];
}
