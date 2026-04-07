using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeValidationFactory
{
    IStripeValidationStrategy Create(ContractType contractType);
}
