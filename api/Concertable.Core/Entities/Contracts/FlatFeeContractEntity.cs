using Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public class FlatFeeContractEntity : ContractEntity
{
    public override ContractType ContractType => ContractType.FlatFee;
    public decimal Fee { get; set; }
}
