using Core.Enums;

namespace Application.Interfaces.Concert;

public interface IPaymentRecipientResolverFactory
{
    IPaymentRecipientResolver Create(ContractType contractType);
}
