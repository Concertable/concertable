using Core.Entities;
using Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public abstract class ContractEntity : BaseEntity
{
    public PaymentMethod PaymentMethod { get; set; }
    public abstract ContractType ContractType { get; }
    public ConcertOpportunityEntity Opportunity { get; set; } = null!;
}
