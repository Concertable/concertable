using Concertable.Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public class VersusContractEntity : ContractEntity
{
    public override ContractType ContractType => ContractType.Versus;
    public decimal Guarantee { get; set; }
    public decimal ArtistDoorPercent { get; set; }
}
