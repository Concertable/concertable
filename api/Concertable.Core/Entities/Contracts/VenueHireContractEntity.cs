using Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public class VenueHireContractEntity : ContractEntity
{
    public override ContractType ContractType => ContractType.VenueHire;
    public decimal HireFee { get; set; }
}
