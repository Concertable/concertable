using Core.Enums;

namespace Concertable.Core.Entities.BookingContracts;

public class DoorSplitContractEntity : BookingContractEntity
{
    public override ContractType ContractType => ContractType.DoorSplit;
    public decimal ArtistDoorPercent { get; set; }
}
