using Core.Enums;

namespace Concertable.Core.Entities.BookingContracts;

public class FlatFeeContractEntity : BookingContractEntity
{
    public override ContractType ContractType => ContractType.FlatFee;
    public decimal Fee { get; set; }
}
