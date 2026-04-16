using Concertable.Core.Entities;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public abstract class ContractEntity : IIdEntity
{
    public int Id { get; set; }
    public PaymentMethod PaymentMethod { get; protected set; }
    public abstract ContractType ContractType { get; }
    public OpportunityEntity Opportunity { get; protected set; } = null!;
}
