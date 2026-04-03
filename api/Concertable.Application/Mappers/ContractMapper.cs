using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;

namespace Concertable.Application.Mappers;

public class ContractMapper : IContractMapper
{
    private readonly IDictionary<ContractType, IContractMapper> mappers = new Dictionary<ContractType, IContractMapper>
    {
        { ContractType.FlatFee, new FlatFeeContractMapper() },
        { ContractType.DoorSplit, new DoorSplitContractMapper() },
        { ContractType.Versus, new VersusContractMapper() },
        { ContractType.VenueHire, new VenueHireContractMapper() }
    };

    public IContract ToDto(ContractEntity entity) => mappers[entity.ContractType].ToDto(entity);
    public ContractEntity ToEntity(IContract dto) => mappers[dto.ContractType].ToEntity(dto);
}
