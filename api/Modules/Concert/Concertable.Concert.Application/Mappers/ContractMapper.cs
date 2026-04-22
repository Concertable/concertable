using System.Collections.Frozen;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class ContractMapper : IContractMapper
{
    private static readonly FrozenDictionary<ContractType, IContractMapper> mappers =
        new Dictionary<ContractType, IContractMapper>
        {
            [ContractType.FlatFee] = new FlatFeeContractMapper(),
            [ContractType.DoorSplit] = new DoorSplitContractMapper(),
            [ContractType.Versus] = new VersusContractMapper(),
            [ContractType.VenueHire] = new VenueHireContractMapper(),
        }.ToFrozenDictionary();

    public IContract ToDto(ContractEntity entity) => mappers[entity.ContractType].ToDto(entity);
    public ContractEntity ToEntity(IContract dto) => mappers[dto.ContractType].ToEntity(dto);
}
