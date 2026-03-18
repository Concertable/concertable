using Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public class DoorSplitContractEntity : ContractEntity
{
    public override ContractType ContractType => ContractType.DoorSplit;
    public decimal ArtistDoorPercent { get; set; }
}
