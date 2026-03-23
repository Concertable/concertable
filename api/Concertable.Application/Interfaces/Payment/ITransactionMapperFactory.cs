using Core.Enums;

namespace Application.Interfaces.Payment;

public interface ITransactionMapperFactory
{
    ITransactionMapper Create(TransactionType type);
}
