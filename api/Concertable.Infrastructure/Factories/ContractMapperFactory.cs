using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Enums;

namespace Infrastructure.Factories;

public class ContractMapperFactory : IContractMapperFactory
{
    private readonly IDictionary<ContractType, IContractMapper> mappers = new Dictionary<ContractType, IContractMapper>
    {
        { ContractType.FlatFee, new FlatFeeContractMapper() },
        { ContractType.DoorSplit, new DoorSplitContractMapper() },
        { ContractType.Versus, new VersusContractMapper() },
        { ContractType.VenueHire, new VenueHireContractMapper() }
    };

    public IContractMapper Create(ContractType type) => mappers[type];
}
