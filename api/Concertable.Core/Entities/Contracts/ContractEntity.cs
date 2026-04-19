using Concertable.Core.Enums;

namespace Concertable.Core.Entities.Contracts;

public abstract class ContractEntity : IIdEntity
{
    protected ContractEntity() { }

    public int Id { get; private set; }
    public PaymentMethod PaymentMethod { get; protected set; }
    public abstract ContractType ContractType { get; }
    public OpportunityEntity Opportunity { get; protected set; } = null!;
}
