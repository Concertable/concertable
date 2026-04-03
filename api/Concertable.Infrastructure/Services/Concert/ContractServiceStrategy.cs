using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;

namespace Concertable.Infrastructure.Services.Concert;

public class ContractServiceStrategy : IContractServiceStrategy
{
    private readonly IDictionary<ContractType, IContractServiceStrategy> strategies = new Dictionary<ContractType, IContractServiceStrategy>
    {
        { ContractType.FlatFee, new FlatFeeContractService() },
        { ContractType.DoorSplit, new DoorSplitContractService() },
        { ContractType.Versus, new VersusContractService() },
        { ContractType.VenueHire, new VenueHireContractService() }
    };

    public void ApplyChanges(ContractEntity existing, IContract dto) => strategies[existing.ContractType].ApplyChanges(existing, dto);
}
