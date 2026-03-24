using Core.Entities;
using Core.Entities.Interfaces;
using Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public abstract class ContractEntity : IEntity
{
    public int Id { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public abstract ContractType ContractType { get; }
    public ConcertOpportunityEntity Opportunity { get; set; } = null!;
}
