using Core.Enums;

namespace Concertable.Core.Entities.BookingContracts;

public class VersusContractEntity : BookingContractEntity
{
    public override ContractType ContractType => ContractType.Versus;
    public decimal Guarantee { get; set; }
    public decimal ArtistDoorPercent { get; set; }
}
