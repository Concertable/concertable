using Core.Enums;

namespace Concertable.Core.Entities.BookingContracts;

public class VenueHireContractEntity : BookingContractEntity
{
    public override ContractType ContractType => ContractType.VenueHire;
    public decimal HireFee { get; set; }
}
